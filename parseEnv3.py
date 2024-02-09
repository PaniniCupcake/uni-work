import main_write
import main_read
import numpy as np

def getObservation():
    readval = ""
    send_ver = 1 - int(main_read.read()[0])
    receive_ver = 1 - send_ver
    #print(receive_ver)
    main_write.write(str(send_ver))
    #print("Obs Recevieing")
    #print(receive_ver)
    i = 3
    while True:
        print(":!")
        try:
            f = open("../EnvData.txt","r")
            readval = f.read()
            #print(readval)
            i = int(readval[0]) 
        except Exception as e:
            #print("Tryna read during write")
            #print(e)
            continue
        f.close()
        if i == receive_ver:
            break
    readval = np.genfromtxt("../EnvData.txt",delimiter=',')
    print(len(readval))
    score = 0
    ob = readval[6:]
    score = int(readval[4]);
    obs = np.array(ob,dtype=np.int8)
    return obs,score,False 

def ActAndGetObservation(action):
    send_ver = 1 - int(main_read.read()[0])
    receive_ver = 1 - send_ver
    main_write.write(str(send_ver) + str(action))
    #print("Wow We actually got  this far")
    readval = ""
    i = 3
    while True:
        print(":5")
        try:
            f = open("../EnvData.txt","r")
            readval = f.read()
            #print(readval)
            i = int(readval[0]) 
        except Exception as e:
            #print("Tryna read during write")
            #print(e)
            continue
        f.close()
        if i == receive_ver:
            break
    readval = np.genfromtxt("../EnvData.txt",delimiter=',')
    print(len(readval))
    done = (int(readval[2]) == 1)
    ob = readval[6:]
    score = int(readval[4]);
    obs = np.array(ob,dtype=np.int8)
    return obs,score,False 

