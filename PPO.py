import tensorflow._api.v2.compat.v1 as tf
import keras
from keras import backend as K
from keras.layers import Activation, Dense, Input,LeakyReLU
from keras.models import Model, load_model
import numpy as np
import tensorflow_probability
tf.disable_v2_behavior()

class ActorNetwork(keras.Model):
    def __init__(self, actions, layer1_nodes=800, layer2_nodes=400):
        super(ActorNetwork, self).__init__()

        self.h1 = Dense(layer1_nodes, activation=LeakyReLU(alpha=0.1))
        self.h2 = Dense(layer2_nodes, activation=LeakyReLU(alpha=0.1))
        self.o = Dense(actions, activation='softmax')

    def call(self, state):
        l1 = self.h1(state)
        l2 = self.h2(l1)
        l3 = self.o(l2)

        return l3


class CriticNetwork(keras.Model):
    def __init__(self, layer1_nodes=800, layer2_nodes=400):
        super(CriticNetwork, self).__init__()
        self.h1 = Dense(layer1_nodes, activation=LeakyReLU(alpha=0.1))
        self.h2 = Dense(layer2_nodes, activation=LeakyReLU(alpha=0.1))
        self.o = Dense(1, activation=None)

    def call(self, state):
        l1 = self.h1(state)
        l2 = self.h2(l1)
        l3 = self.o(l2)

        return l3

class PPOMemory:
    def __init__(self, batch_size):
        self.states = []
        self.probs = []
        self.vals = []
        self.actions = []
        self.rewards = []
        self.dones = []

        self.batch_size = batch_size

    def generate_batches(self):
        n_states = len(self.states)
        batch_start = np.arange(0, n_states, self.batch_size)
        indices = np.arange(n_states, dtype=np.int64)
        np.random.shuffle(indices)
        batches = [indices[i:i+self.batch_size] for i in batch_start]

        return np.array(self.states), np.array(self.actions), np.array(self.probs),np.array(self.vals),np.array(self.rewards), np.array(self.dones), batches

    def store_memory(self, state, action, probs, vals, reward, done):
        self.states.append(state)
        self.actions.append(action)
        self.probs.append(probs)
        self.vals.append(vals)
        self.rewards.append(reward)
        self.dones.append(done)

    def clear_memory(self):
        self.states = []
        self.probs = []
        self.actions = []
        self.rewards = []
        self.dones = []
        self.vals = []

class Agent:
    def __init__(self, n_actions, input_dims, gamma=0.99, alpha=0.0003,
                 gae_lambda=0.95, policy_clip=0.2, batch_size=64,
                 n_epochs=10):
        self.gamma = gamma
        self.policy_clip = policy_clip
        self.n_epochs = n_epochs
        self.gae_lambda = gae_lambda

        self.actor = ActorNetwork(n_actions)
        self.actor.compile(optimizer=tf.keras.optimizers.Adam(learning_rate=alpha))
        self.critic = CriticNetwork()
        self.critic.compile(optimizer=tf.keras.optimizers.Adam(learning_rate=alpha))
        self.memory = PPOMemory(batch_size)

    def store_transition(self, state, action, probs, vals, reward, done):
        self.memory.store_memory(state, action, probs, vals, reward, done)

    def save_models(self):
        self.actor.save('a')
        self.critic.save('c')

    def load_models(self):
        self.actor = keras.models.load_model(self.chkpt_dir + 'a')
        self.critic = keras.models.load_model(self.chkpt_dir + 'c')

    def choose_action(self, observation):
        state = tf.convert_to_tensor([observation])

        probs = self.actor(state)
        dist = tensorflow_probability.distributions.Categorical(probs)
        action = dist.sample()
        log_prob = dist.log_prob(action)
        value = self.critic(state)

        action = action.numpy()[0]
        value = value.numpy()[0]
        log_prob = log_prob.numpy()[0]

        return action, log_prob, value

    def learn(self):
        for _ in range(self.n_epochs):
            state_arr, action_arr, old_prob_arr, vals_arr,reward_arr, dones_arr, batches = self.memory.generate_batches()

            values = vals_arr
            advantage = np.zeros(len(reward_arr), dtype=np.float32)

            for t in range(len(reward_arr)-1):
                discount = 1
                a_t = 0
                for k in range(t, len(reward_arr)-1):
                    a_t += discount*(reward_arr[k] + self.gamma*values[k+1] * (
                        1-int(dones_arr[k])) - values[k])
                    discount *= self.gamma*self.gae_lambda
                advantage[t] = a_t

            act_loss_tot = 0
            crit_loss_tot = 0

            for batch in batches:
                with tf.GradientTape(persistent=True) as tape:
                    states = tf.convert_to_tensor(state_arr[batch])
                    old_probs = tf.convert_to_tensor(old_prob_arr[batch])
                    actions = tf.convert_to_tensor(action_arr[batch])

                    probs = self.actor(states)
                    dist = tensorflow_probability.distributions.Categorical(probs)
                    new_probs = dist.log_prob(actions)

                    critic_value = self.critic(states)

                    critic_value = tf.squeeze(critic_value, 1)

                    prob_ratio = tf.math.exp(new_probs - old_probs)
                    weighted_probs = advantage[batch] * prob_ratio
                    clipped_probs = tf.clip_by_value(prob_ratio,
                                                     1-self.policy_clip,
                                                     1+self.policy_clip)
                    weighted_clipped_probs = clipped_probs * advantage[batch]
                    actor_loss = -tf.math.minimum(weighted_probs,
                                                  weighted_clipped_probs)
                    actor_loss = tf.math.reduce_mean(actor_loss)

                    returns = advantage[batch] + values[batch]
                    # critic_loss = tf.math.reduce_mean(tf.math.pow(
                    #                                  returns-critic_value, 2))
                    critic_loss = keras.losses.MSE(critic_value, returns)

                    act_loss_tot += actor_loss
                    crit_loss_tot += critic_loss

                actor_params = self.actor.trainable_variables
                actor_grads = tape.gradient(actor_loss, actor_params)
                critic_params = self.critic.trainable_variables
                critic_grads = tape.gradient(critic_loss, critic_params)
                self.actor.optimizer.apply_gradients(
                        zip(actor_grads, actor_params))
                self.critic.optimizer.apply_gradients(
                        zip(critic_grads, critic_params))

        self.memory.clear_memory()
        act_loss_tot /= len(batches)
        crit_loss_tot /= len(batches)
        return act_loss_tot,crit_loss_tot
