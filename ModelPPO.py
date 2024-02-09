import numpy as np
import tensorflow as tf
from tensorflow import keras
from keras import layers
import gym
from gym import Env
import Itb_Gym
import main_write
import main_read
from PPO import Agent
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
    f = open("../EnvData.txt", "w")
    f.write("1")
    f.close()
    main_write.write(0)
    env = gym.make("Itb-v0")
    test = False
    agent = Agent(alpha=0.00001,beta=0.00005)

    if test:
        agent.load_model()
    scores = []
    eps_history = []
    err_history = []
    err_history2 = []
    avg_history = []
    steps = 0
    actions_taken = 0
    for i in range(40000):
        done = False
        score = 0
        obs,_= env.reset()
        observation = obs   
        err = 0
        err2 = 0
        while not done:
            observation = env.reset()
            done = False
            score = 0
            while not done:
                action, prob, val = agent.choose_action(observation)
                observation_, reward, done, info = env.step(action)
                steps += 1
                score += reward
                if not test:
                    agent.store_transition(observation, action,
                                        prob, val, reward, done)
                    if steps % 20 == 0:
                        e1,e2 = agent.learn()
                        err += e1
                        err2 += e2
                observation = observation_
            actions_taken += 1

        err_history.append(err/actions_taken)
        err_history2.append(err2/actions_taken)
        actions_taken = 0
        scores.append(score)
        avg_score = np.mean(scores[-100:])
        avg_history.append(avg_score)
        print('episode ',i, "score %.2f" % score, "average score %.2f" % avg_score)
        if i % 500 == 0 and i > 0:
            if not test:
                agent.save_models()
            x = range(i+1)
            plot_learning_curve(x,scores,"Score",'graph5.png')
            plot_learning_curve(x,err_history,"Act Loss",'graph5b.png')
            plot_learning_curve(x,err_history2,"Crit Loss",'graph5c.png')
            plot_learning_curve(x,avg_history,"Average score",'graph5d.png')

    print(datetime.now() - startTime)
    x = range(40000)
    plot_learning_curve(x,scores,"Score",'graph5.png')
    plot_learning_curve(x,err_history,"Act Loss",'graph5b.png')
    plot_learning_curve(x,err_history2,"Crit Loss",'graph5c.png')
    plot_learning_curve(x,avg_history,"Average score",'graph5d.png')



