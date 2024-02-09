import tensorflow._api.v2.compat.v1 as tf
from keras import backend as K
from keras.layers import Activation, Dense, Input,LeakyReLU
from keras.models import Model, load_model
import numpy as np
from keras.utils.generic_utils import get_custom_objects
tf.disable_v2_behavior()


class ActCritAgent(object):
    def __init__(self,alpha,beta,gamma=0.99,n_actions=245,input_dims=380):
        self.gamma = gamma
        self.alpha = alpha
        self.beta = beta
        self.input_dims =input_dims
        self.n_actions = n_actions
        self.actor, self.critic, self.policy=self.build_ac()
        self.action_space = [i for i in range(self.n_actions)]
         
    def build_ac(self):
        input = Input(shape=(self.input_dims,))
        delta = Input(shape=[1])
        dense1 = Dense(800,activation=LeakyReLU(alpha=0.1))(input)
        dense2 = Dense(400,activation=LeakyReLU(alpha=0.1))(dense1)
        probs = Dense(self.n_actions, activation='softmax')(dense2)
        values = Dense(1,activation='linear')(dense2)

        def acloss(y_true,y_pred):
            out = K.clip(y_pred,1e-8,1-1e-8)
            log_likelihood = y_true*K.log(out)
            return K.sum(-log_likelihood*delta)

        actor = Model(inputs=[input,delta], outputs=[probs])
        actor.compile(optimizer=tf.keras.optimizers.Adam(lr=self.alpha),loss=acloss,experimental_run_tf_function=False)

        critic = Model(inputs = [input],outputs=[values])
        critic.compile(optimizer=tf.keras.optimizers.Adam(lr=self.beta),loss='mean_squared_error',experimental_run_tf_function=False)

        policy = Model(inputs=[input],outputs=[probs])

        return actor,critic,policy

    def choose_action(self,observation):
        state=observation[np.newaxis,:]
        probabilities= self.policy.predict(state)[0]
        probs = np.array(probabilities)
        probs = np.clip(probs, 1e-8,1-1e-8)


        valid_actions = []
        valid_probabilities = []
        readval = ""
        while(len(readval) != 245):
            try:
                f = open("../ActData.txt","r")
                readval = f.read()
                break;
            except Exception as e:
                    #print("Tryna read during write")
                    #print(e)
                continue
            f.close()
        for i in range(245):
            if int(readval[i]) == 1 and not np.isnan(probs[i]):
                valid_actions.append(i)
                valid_probabilities.append(probs[i])
                
        valid_probabilities /= sum(valid_probabilities)
        #print(valid_probabilities)
        action = np.random.choice(valid_actions,p=valid_probabilities)
        return action
    
    def learn(self,state,action,reward,state_,done):

        
        state=state[np.newaxis,:]
        state_ = state_[np.newaxis,:]
        critic_value = self.critic.predict(state)
        critic_value_ = self.critic.predict(state_)
        target = reward+self.gamma*critic_value_*(1-int(done))
        delta = target - critic_value
        
        actions = np.zeros([1,self.n_actions])
        actions[np.arange(1),action] = 1.0
        history = self.actor.fit([state,delta],actions,verbose=0)
        history2 = self.critic.fit(state,target,verbose=0)
        return history.history['loss'][0],history2.history['loss'][0]


    def load_model(self):
        delta = Input(shape=[1])
        def acloss(y_true,y_pred):
            out = K.clip(y_pred,1e-8,1-1e-8)
            log_likelihood = y_true*K.log(out)
            return K.sum(-log_likelihood*delta)
        a = load_model('actor.h5', custom_objects={'acloss':acloss})
        c = load_model('critic.h5')
        p = load_model('policy.h5')
        self.actor.set_weights(a.get_weights())
        self.critic.set_weights(c.get_weights())
        self.policy.set_weights(p.get_weights())

    def save_model(self):
        self.actor.save('actor.h5')
        self.critic.save('critic.h5')
        self.policy.save('policy.h5')
