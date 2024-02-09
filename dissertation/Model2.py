import numpy as np
import tensorflow as tf
from tensorflow import keras
from keras import layers
import gym
from gym import Env
import Itb_Gym
import main_write
import main_read

import RainAgent
import sys
from datetime import datetime
import signal
import parseEnv2
import matplotlib.pyplot as plt
from typing import Any, Optional, Tuple

def plot_learning_curve(x,scores,name,filename,lines=None):
     fig = plt.figure()
     ax = fig.add_subplot(111,label="1")
     ax.plot(x,scores,'ro',color="C0", markersize=1)
     ax.set_xlabel("Game",color="C0")
     ax.set_ylabel(name,color="C0")
     ax.tick_params(axis='x',color="C0")
     ax.tick_params(axis='y',color="C0")
     plt.savefig(filename)


if __name__ == '__main__':
    #Reset interprocess comms
    original_sigint = signal.getsignal(signal.SIGINT)
    startTime = datetime.now()
    tf.config.list_physical_devices('GPU')
    assert tf.test.is_built_with_cuda()
    f = open("../EnvData.txt", "w")
    f.write("1")
    f.close()
    main_write.write(0)
    env = gym.make("Itb-v0")
    test = False
    #agent = Agent.Agent(gamma = 0.99, epsilon=1, input_dims = 380, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0.025)
    #agent = MultiAgent.Agent(gamma = 0.99, epsilon=1, input_dims =380, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0.025)
    #agent = DuelAgent2.Agent(gamma = 0.99, epsilon=1, input_dims = 380, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0.025)
    if test:
        agent = RainAgent.Agent(gamma = 0.99, epsilon=0, input_dims = 380, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0)
    else:
        agent = RainAgent.Agent(gamma = 0.99, epsilon=1, input_dims = 380, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0.025)

    if test:
        agent.load_model()
    scores = []
    eps_history = []
    err_history = []
    avg_history = []
    actions_taken = 0
    for i in range(20001):
        done = False
        score = 0
        obs,_= env.reset()
        observation = obs   
        err = 0
        while not done:
            action = agent.choose_action(observation)
            print("New action " + str(actions_taken))
            print(action)
            #if action == -1:
            #    action = parseEnv2.GetBest("t")
            #    print(action)
            #elif action == -2:
            #    action = parseEnv2.GetBest("r")
            #    print(action)
            obs_, reward, done, info = env.step(action)
            observation_ = obs_
            score += reward
            if not test:
                print(reward)
                agent.remember(observation,action,reward,observation_,done)
                observation = observation_
                err += agent.learn()
                print(err)
            actions_taken += 1
        err_history.append(err/actions_taken)
        actions_taken = 0
        eps_history.append(agent.epsilon)
        scores.append(score)
        avg_score = np.mean((scores))
        avg_history.append(avg_score)
        print('episode ',i, "score %.2f" % score, "average score %.2f" % avg_score)
        print('epsilon:')
        print('buffer length: ' , agent.memory.mem_cntr)
        print(agent.getEpsilon())
        if i % 500 == 0 and i > 0:
            if not test:
                agent.save_model()
            if agent.memory.mem_cntr > agent.memory.mem_size:
                agent.memory.mem_cntr = agent.memory.mem_cntr % agent.memory.mem_size + agent.memory.mem_size
            x = range(i+1)
            plot_learning_curve(x,scores,"Score",'graph5.png')
            plot_learning_curve(x,err_history,"Loss",'graph5b.png')
            plot_learning_curve(x,avg_history,"Average score",'graph5c.png')
    #np.savetxt("sm.csv", agent.memory.state_memory, delimiter=",")
    #np.savetxt("nsm.csv",  agent.memory.new_state_memory, delimiter=",")
    #np.savetxt("am.csv",  agent.memory.action_memory, delimiter=",")
    #np.savetxt("rm.csv",  agent.memory.reward_memory, delimiter=",")
    #np.savetxt("tm.csv",  agent.memory.terminal_memory, delimiter=",")
    #np.savetxt("tn.csv",  agent.memory.terminal_next, delimiter=",")
    #np.savetxt("lm.csv",  agent.memory.legal_memory, delimiter=",")
    #np.savetxt("p.csv",  agent.memory.priorities, delimiter=",")
    #np.savetxt("rn.csv", agent.memory.reward_next, delimiter=",")

    print(datetime.now() - startTime)
    x = range(20001)
    plot_learning_curve(x,scores,"Score",'graph5.png')
    plot_learning_curve(x,err_history,"Loss",'graph5b.png')
    plot_learning_curve(x,avg_history,"Average score",'graph5c.png')


