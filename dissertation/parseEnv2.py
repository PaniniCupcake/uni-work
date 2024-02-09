import main_write
import main_read
import numpy as np
def parseEnv(s):
    ss = ""
    ns = 0
    for i in s:
      if(i == ','):
        break
      if(i == '\n'):
        ns += 1
        continue
      ss += i

    s = s[len(ss)+1 + ns:]
    return ss,s 

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

    count = 0
    score = 0
    readval = readval[2:]
    ob = []
    s = ""
    s,readval = parseEnv(readval)
    score = int(s);
    ob = []
    for i in range(380):
        s,readval = parseEnv(readval)
        ob.append(int(s))
    if len(readval) > 0:
        print("TOO LONG!")


    
    obs = np.array(ob,dtype=np.int8)
    return obs,score,False 

def ActAndGetObservation(action):
    send_ver = 1 - int(main_read.read()[0])
    receive_ver = 1 - send_ver
    main_write.write(str(send_ver) + str(action))
    #print("Wow We actually got  this far")
    readval = ""
    i = 3
    while(True):
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

    count = 0
    score = 0
    done = (int(readval[1]) == 1)
    #print(done)
    readval = readval[2:]
    ob = []
    s = ""
    s,readval = parseEnv(readval)
    #score += int(s) * 20000000
    #s,readval = parseEnv(readval)
    #score += int(s) * 1000000
    #s,readval = parseEnv(readval)
    #score += int(s) * 1000
    #s,readval = parseEnv(readval)
    #score += int(s) * 100
    #s,readval = parseEnv(readval)
    #score += int(s)
    #s,readval = parseEnv(readval)
    #score += int(s)
    score = int(s);
    ob = []
    for i in range(380):
        s,readval = parseEnv(readval)
        ob.append(int(s))
    if len(readval) > 0:
        print("TOO LONG!")

    
    obs = np.array(ob,dtype=np.int8)
    return obs,score,done

def GetBest(type):
    send_ver = 1 - int(main_read.read()[0])
    receive_ver = 1 - send_ver
    main_write.write(str(send_ver) + type)
    readval = ""
    i = 3
    while(True):
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
    #print(done)
    readval = readval[1:]
    return int(readval)