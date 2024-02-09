import tensorflow as tf
import keras
from keras.layers import Input, Dense, Lambda, Add
from keras.models import Model, Sequential, load_model
import numpy as np
from keras import backend as K

def DuelAgent(input_shape, action_space):
    X_input = Input(input_shape)
    X = X_input

    # 'Dense' is the basic form of a neural network layer
    # Input Layer of state size(4) and Hidden Layer with 512 nodes
    X = Dense(512, input_shape=(input_shape,), activation="relu", kernel_initializer='he_uniform')(X)

    # Hidden layer with 256 nodes
    X = Dense(256, activation="relu", kernel_initializer='he_uniform')(X)

    state_value = Dense(1, kernel_initializer='he_uniform')(X)
    state_value = Lambda(lambda s: K.expand_dims(s[:, 0], -1), output_shape=(action_space,))(state_value)

    action_advantage = Dense(action_space, kernel_initializer='he_uniform')(X)
    action_advantage = Lambda(lambda a: a[:, :] - K.mean(a[:, :], keepdims=True), output_shape=(action_space,))(action_advantage)

    X = Add()([state_value, action_advantage])
    model = Model(inputs = X_input, outputs = X)
    model.compile(loss="mean_squared_error", optimizer="adam")

    model.summary()
    return model

class ReplayBuffer(object):
    def __init__(self,max_size,input_shape):
        self.mem_size = max_size
        self.mem_cntr = 0
        self.state_memory = np.zeros((self.mem_size,input_shape), dtype = np.int8)
        self.new_state_memory = np.zeros((self.mem_size,input_shape), dtype = np.int8)
        self.action_memory = np.zeros((self.mem_size), dtype=np.int16)
        self.reward_memory = np.zeros(self.mem_size)
        self.terminal_memory = np.zeros(self.mem_size,dtype=np.int16)
        self.legal_memory = np.zeros((self.mem_size,245), dtype = np.int8)
        self.cur_legal = np.zeros((245), dtype = np.int8)
        
    def store_transition(self,state,action,reward,state_,done):
        self.mem_cntr %= self.mem_size
        index = self.mem_cntr
        self.state_memory[index] = state
        self.new_state_memory[index] = state_
        self.reward_memory[index] = reward
        if(action == 244):
            self.terminal_memory[index] = 1 - int(done)
        else:
            self.terminal_memory[index] = ((1 - int(done)) * 1000)
        self.action_memory[index] = action
        self.legal_memory[index] = self.cur_legal[:]
        self.mem_cntr += 1

    def sample_buffer(self,batch_size):
        max_mem = min(self.mem_cntr,self.mem_size)
        batch = np.random.choice(max_mem,batch_size,replace=False)
        states = self.state_memory[batch]
        states_ = self.new_state_memory[batch]
        rewards = self.reward_memory[batch]
        actions = self.action_memory[batch]
        terminal = self.terminal_memory[batch]
        legal = self.legal_memory[batch]
        return states,actions,rewards,states_,terminal, legal


class Agent(object):
    def __init__(self,gamma,n_actions,epsilon,batch_size,input_dims,epsilon_dec=0.999,epsilon_end=0.01,mem_size=1000000,fname='ddqn_model.pb',replace = 100):
        self.action_space = [i for i in range(n_actions)]
        self.n_actions = n_actions
        self.gamma = gamma
        self.epsilon = epsilon
        self.epsilon_dec = epsilon_dec
        self.epsilon_min = epsilon_end
        self.batch_size = batch_size
        self.model_file = fname
        self.memory = ReplayBuffer(mem_size,input_dims)
        self.replace = replace
        self.learn_step_counter = 0
        self.q_eval=DuelAgent(input_dims,n_actions)
        self.q_next=DuelAgent(input_dims,n_actions)
        self.q_eval.compile(optimizer="adam",loss='mse')
        self.q_next.compile(optimizer="adam",loss='mse')
    
    def remember(self,state,action,reward,new_state,done):
        self.memory.store_transition(state,action,reward,new_state,done)

    def choose_action(self,state):
        state = state[np.newaxis, :]
        rand = np.random.random()
        valid_actions = []
        #print("Read length")
        #print(len(readval))
        #print(readval)
        if rand < self.epsilon:
            #rand = np.random.random()
            if True:
                readval = "";
                while(len(readval) != 245):
                    try:
                        f = open("../ActData.txt","r")
                        readval = f.read()
                        break
                    except Exception as e:
                        #print("Tryna read during write")
                        #print(e)
                        continue
                    f.close()
                for i in range(245):
                    self.memory.cur_legal[i] = int(readval[i])
                    if int(readval[i]) == 1:
                        valid_actions.append(int(i))
                action = np.random.choice(valid_actions)
            elif rand <= 0.3:
                action = -1
            else:
                action = -2
            #print(rand)   
            #print("Random actions")
            #print(valid_actions)
        else:
            #print("Greed")
            readval = ""
            while(len(readval) != 245):
                try:
                    f = open("../ActData.txt","r")
                    readval = f.read()
                    break
                except Exception as e:
                    #print("Tryna read during write")
                    #print(e)
                    continue
                f.close()
            actions = self.q_eval.predict(state,verbose = 0)
            for i in range(245):
                self.memory.cur_legal[i] = int(readval[i])
                if int(readval[i]) == 0:
                    actions[0][i] = -20000000
                    
            action = np.argmax(actions)
        #print("Taking action")
        #print(action)
        return action

    def learn(self):
        if self.memory.mem_cntr < self.batch_size:
            return
        
        if self.learn_step_counter % self.replace == 0:
            self.q_next.set_weights(self.q_eval.get_weights())
            self.learn_step_counter = 0

        state,action,reward,new_state,done, legal = self.memory.sample_buffer(self.batch_size)

        action_values = np.array(self.action_space, dtype=np.int16)

        #action_indices = np.dot(action,action_values)
        q_pred = self.q_eval(state)
        q_next = self.q_eval(new_state).numpy()
        q_target = q_pred.numpy()
        q_next[legal==0] = -20000000
        
        batch_index = np.arange(self.batch_size, dtype=np.int16)

        q_target[batch_index,action] = reward + self.gamma*np.amax(q_next,axis=1)*(done/1000)

        self.q_eval.fit(state,q_target,batch_size = 64, verbose = 0)

        self.epsilon = self.epsilon * self.epsilon_dec if self.epsilon > self.epsilon_min else self.epsilon_min

        self.learn_step_counter += 1

    def save_model(self):
        self.q_eval.save(self.model_file,save_format="tf")

    def load_model(self):
        self.q_eval = load_model(self.model_file)

    def getEpsilon(self):
        return self.epsilon
