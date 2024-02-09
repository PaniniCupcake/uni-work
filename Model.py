import numpy as np
import tensorflow as tf
from tensorflow import keras
from keras import layers
import gym
from gym import Env
import Itb_Gym
import main_write
import main_read
import Agent
import sys
from datetime import datetime

#from utils import plotLearning



if __name__ == '__main__':
    #Reset interprocess comms
    startTime = datetime.now()
    tf.config.list_physical_devices('GPU')
    assert tf.test.is_built_with_cuda()
    f = open("../EnvData.txt", "w")
    f.write("1")
    f.close()
    main_write.write(0)
    env = gym.make("Itb-v0")
    n_games = 500
    agent = Agent.Agent(alpha = 0.0005,gamma = 0.99, epsilon=1, input_dims = 717, n_actions = 245, mem_size = 1000000, batch_size = 64, epsilon_end = 0.01)

    #agent.load_model()
    scores = []
    eps_history = []

    for i in range(50000):
        break
        done = False
        score = 0
        obs,_= env.reset()
        observation = obs
        while not done:
            action = agent.choose_action(observation)
            #print(action)
            obs_, reward, done, info = env.step(action)
            observation_ = obs_
            score += reward
            #print(reward)
            agent.remember(observation,action,reward,observation_,done)
            observation = observation_
            agent.learn()
        eps_history.append(agent.epsilon)
        scores.append(score)
        avg_score = np.mean((scores[max(0,i-100):(i+1)]))
        print('episode ',i, "score %.2f" % score, "average score %.2f" % avg_score)
        print('epsilon:')
        print(agent.getEpsilon())
        if i % 100 == 0 and i > 0:
            agent.save_model()
            pass
        
    agent.save_model()
    print('epsilon:')
    print(agent.getEpsilon())
    x = [i+1 for i in range(n_games)]
    filename = 'itb.png'
    print(datetime.now() - startTime)
    #plotLearning(x,scores,eps_history,filename)

