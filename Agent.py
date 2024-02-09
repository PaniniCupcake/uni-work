from keras.layers import Dense, Activation
from keras.models import Sequential, load_model
import numpy as np
import random
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
        self.priorities = np.zeros((self.mem_size))
        self.priorities[0] = 1

    def store_transition(self,state,action,reward,state_,done):
        index = self.mem_cntr % self.mem_size
        self.state_memory[index] = state
        self.new_state_memory[index] = state_
        self.reward_memory[index] = reward
        if(action == 244):
            self.terminal_memory[index] = 1 - int(done)
        else:
            self.terminal_memory[index] = ((1 - int(done)) * 1000)
        self.action_memory[index] = action
        self.mem_cntr += 1
        self.legal_memory[index] = self.cur_legal[:]
        self.priorities[index] = max(self.priorities)

    def get_probabilities(self,pri_scale,max_mem):
        pris = self.priorities[:max_mem]
        scaled_priorities = pris ** pri_scale
        sample_probs = scaled_priorities / sum(scaled_priorities)
        return sample_probs

    def set_priorities(self, indices,e, offset = 0.1):
        for i in indices:
            self.priorities[i] = abs(e) + offset


    def sample_buffer(self,batch_size,priority_scale = 1.0):
        max_mem = min(self.mem_cntr,self.mem_size)
        sample_prob = self.get_probabilities(priority_scale,max_mem)
        batch = random.choices(range(max_mem),weights=sample_prob,k=batch_size)
        states = self.state_memory[batch]
        states_ = self.new_state_memory[batch]
        rewards = self.reward_memory[batch]
        actions = self.action_memory[batch]
        terminal = self.terminal_memory[batch]
        legal = self.legal_memory[batch]
        importance = self.get_importance(max_mem,sample_prob[batch])
        return states,actions,rewards,states_,terminal, legal, importance, batch
    
    def get_importance(self,size,probs):
        importance = 1/size * 1/probs
        norm_importance = importance / max(importance)
        return norm_importance
#useless
    def save(self):
        try:
            s1 = ""
            s2 = ""
            s3 = ""
            s4 = ""
            s5 = ""
            s6 = ""
            for i in range(self.mem_cntr):
                for j in self.state_memory[i]:
                    s1 += str(j) + ","
                for j in self.new_state_memory[i]:
                    s2 += str(j) + ","
                s3 += str(self.action_memory[i]) + ","
                s4 += str(self.reward_memory[i]) + ","
                s5 += str(self.terminal_memory[i]) + ","
                for j in self.legal_memory[i]:
                    s6 += str(j) + ","
            print("Done writing ")
            sm = open("States.txt", "w")
            sm.write(s1)
            sm.close()
            nm = open("NewStates.txt", "w")
            nm.write(s2)
            nm.close()
            am = open("Actions.txt", "w")
            am.write(s3)
            am.close()
            rm = open("Rewards.txt", "w")
            rm.write(s4)
            rm.close()
            tm = open("Dones.txt", "w")
            tm.write(s5)
            tm.close()
            lm = open("Dones.txt", "w")
            lm.write(s5)
            lm.close()
        except:
            sm.close()
            nm.close()
            am.close()
            rm.close()
            tm.close()

def build_dqn(n_actions, input_dims):
    model = Sequential([
        Dense(512, input_shape=(input_dims,),activation='relu'),
        Dense(512,activation='relu'),
        Dense(n_actions)])
    model.compile(optimizer="adam",loss='mse')
    model.summary()

    return model

class Agent(object):
    def __init__(self,gamma,n_actions,epsilon,batch_size,input_dims,epsilon_dec=0.999995,epsilon_end=0.01,mem_size=1000000,fname='model2.h5'):
        self.action_space = [i for i in range(n_actions)]
        self.n_actions = n_actions
        self.gamma = gamma
        self.epsilon = epsilon
        self.epsilon_dec = epsilon_dec
        self.epsilon_min = epsilon_end
        self.batch_size = batch_size
        self.model_file = fname
        self.replace = 100
        self.learn_step_counter = 0
        self.memory = ReplayBuffer(mem_size,input_dims)
        self.q_eval=build_dqn(n_actions,input_dims)
        self.q_next=build_dqn(n_actions,input_dims)
    
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
                        break;
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
            readval = "";
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
            actions = self.q_eval.predict(state,verbose = 0)
            for i in range(245):
                self.memory.cur_legal[i] = int(readval[i])
                if int(readval[i]) == 0:
                    actions[0][i] = -20000000
                    
            #actions[0][0] = -20000000;
            #actions[0][1] = 0;
            #print(actions)
            #print(np.argmax(actions))
           # print("Greedy actions")
            #print(actions)
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
        state,action,reward,new_state,done, legal, importance, indices = self.memory.sample_buffer(self.batch_size)
        action_values = np.array(self.action_space, dtype=np.int16)
        #action_indices = np.dot(action,action_values)

        q_eval = self.q_eval.predict(state)
        q_next = self.q_next.predict(new_state,verbose = 0)

        #for i in range(len(q_next)):
         #   for j in range(len(q_next[i])):
          #      if legal[i][j] == 0: 
           #         q_next[i][j] = -20000000

        q_next[legal==0] = -20000000
        #reward[action == 244] = (reward + q_next[244])/2
        q_target = q_eval.copy()

        batch_index = np.arange(self.batch_size, dtype=np.int16)
        q_target[batch_index,action] = reward + self.gamma*np.max(q_next,axis=1)*(done/1000)

        history = self.q_eval.fit(state, q_target,batch_size = 64, verbose = 0)
        errors = np.sqrt(history.history['loss'])
        self.epsilon = self.epsilon * self.epsilon_dec if self.epsilon > self.epsilon_min else self.epsilon_min
        self.learn_step_counter += 1

        self.memory.set_priorities(indices,errors)

    def save_model(self):
        self.q_eval.save(self.model_file)

    def save_final(self):
        #self.memory.save()
        self.q_eval.save(self.model_file)
        

    def load_model(self):
        self.q_eval = load_model(self.model_file)
        self.q_next = load_model(self.model_file)
        self.q_eval.summary()

    def getEpsilon(self):
        return self.epsilon
