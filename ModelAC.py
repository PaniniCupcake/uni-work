import numpy as np
import tensorflow as tf
from tensorflow import keras
from keras import layers
import gym
from gym import Env
import Itb_Gym
import main_write
import main_read
from ActorCritic import ActCritAgent
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
    test = True
    trainmore = True
    agent = ActCritAgent(alpha=0.00001,beta=0.00005)

    if test or trainmore:
        agent.load_model()
    scores = []
    eps_history = []
    err_history = []
    err_history2 = []
    avg_history = []
    actions_taken = 0
    for i in range(500000):
        done = False
        score = 0
        obs,_= env.reset()
        observation = obs   
        err = 0
        err2 = 0
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
                observation = observation_
                e1,e2= agent.learn(obs,action,reward,obs_,done)
                err += e1
                err2 += e2
            actions_taken += 1

        err_history.append(err/actions_taken)
        err_history2.append(err2/actions_taken)
        actions_taken = 0
        scores.append(score)
        avg_score = np.mean(scores[-100:])
        avg_history.append(avg_score)
        print('episode ',i, "score %.2f" % score, "average score %.2f" % avg_score)
        if i % 1000 == 0 and i > 0:
            if not test:
                agent.save_model()
            x = range(i+1)
            try:
                plot_learning_curve(x,scores,"Score",'graph5.png')
                plot_learning_curve(x,err_history,"Act Loss",'graph5b.png')
                plot_learning_curve(x,err_history2,"Crit Loss",'graph5c.png')
                plot_learning_curve(x,avg_history,"Average score",'graph5d.png')
            except:
                try:   
                    plot_learning_curve(x[-40000:],scores[-40000:],"Score",'graph6.png')
                    plot_learning_curve(x[-40000:],err_history[-40000:],"Act Loss",'graph6b.png')
                    plot_learning_curve(x[-40000:],err_history2[-40000:],"Crit Loss",'graph6c.png')
                    plot_learning_curve(x[-40000:],avg_history[-40000:],"Average score",'graph6d.png')
                except: 
                    pass

                

    print(datetime.now() - startTime)
    x = range(500000)
    try:
        plot_learning_curve(x,scores,"Score",'graph5.png')
        plot_learning_curve(x,err_history,"Act Loss",'graph5b.png')
        plot_learning_curve(x,err_history2,"Crit Loss",'graph5c.png')
        plot_learning_curve(x,avg_history,"Average score",'graph5d.png')
    except:
        try:   
            plot_learning_curve(x[-40000:],scores[-40000:],"Score",'graph6.png')
            plot_learning_curve(x[-40000:],err_history[-40000:],"Act Loss",'graph6b.png')
            plot_learning_curve(x[-40000:],err_history2[-40000:],"Crit Loss",'graph6c.png')
            plot_learning_curve(x[-40000:],avg_history[-40000:],"Average score",'graph6d.png')
        except: 
            pass

