import numpy as np
import main_write
import random
def parseEnv(s):
    ss = ""
    for i in s:
      if(i == ','):
        break
      ss += i
    s = s[len(ss)+1:]
    return ss,s 

def main():
    count = 0
    comms = 0
    i = 0
    send_ver = 0
    receive_ver = 1
    r = str(random.randrange(0,247))
    print("First write")
    print(r)
    main_write.write(str(send_ver) + r)
    f = open("../EnvData.txt", "w")
    random.seed()
    f.write("0")
    f.close()
    while(comms < 100000):
        try:
            f = open("../EnvData.txt","r")
            readval = f.read()
            #print("Receiving " + readval + ", want " + str(receive_ver))
            i = int(readval[0]) 
        except Exception as e:
            print("Tryna read during write")
            print(e)
            continue
        if i == receive_ver:

            comms += 1
            send_ver = (send_ver + 1) % 2
            receive_ver = (receive_ver + 1) % 2
            r = str(random.randrange(0,247))
            print("Next write")
            print(r)
            main_write.write(str(send_ver) + r)
            #print(comms)
            print("Comm no")
            print(comms)
            continue
        f.close()
        count += 1
        #print(count)
    count = 0
    score = 0
    readval = readval[1:]
    s = ""
    print(readval)
    s,readval = parseEnv(readval)
    print(readval)
    score += int(s) * 20000000
    print(s)
    s,readval = parseEnv(readval)
    score += int(s) * 1000000
    print(s)
    s,readval = parseEnv(readval)
    score += int(s) * 1000
    print(s)
    s,readval = parseEnv(readval)
    score += int(s) * 100
    print(s)
    s,readval = parseEnv(readval)
    score += int(s)
    print(s)
    s,readval = parseEnv(readval)
    score += int(s)
    print(s)
    s,readval = parseEnv(readval)
    legalactions = int(s)
    s,readval = parseEnv(readval)
    turn = int(s)
    s,readval = parseEnv(readval)
    squad = int(s)
    s,readval = parseEnv(readval)
    hp = int(s)
    s,readval = parseEnv(readval)
    resist = int(s)
    s,readval = parseEnv(readval)
    perfect = int(s)
    s,readval = parseEnv(readval)
    mission = int(s)
    s,readval = parseEnv(readval)
    progress = int(s)
    tilestates = []
    for i in range(8):
        subarr = []
        for j in range(8):
          s,readval = parseEnv(readval)
          subarr.append(int(s))
        tilestates.append(subarr)
    occupants = []
    for i in range(8):
        subarr = []
        for j in range(8):
          s,readval = parseEnv(readval)
          subarr.append(int(s))
        occupants.append(subarr)
    occhealth = []
    for i in range(8):
        subarr = []
        for j in range(8):
          s,readval = parseEnv(readval)
          subarr.append(int(s))
        occhealth.append(subarr)
    occstatus = []
    for i in range(8):
        subarr = []
        for j in range(8):
          s,readval = parseEnv(readval)
          subarr.append(int(s))
        occstatus.append(subarr)
    attackorder = [[],[]]
    for i in range(24):
      s,readval = parseEnv(readval)
      attackorder[i%2].append(s)
    burrowertiles = [[],[]]
    for i in range(4):
      s,readval = parseEnv(readval)
      burrowertiles[0].append(s)
    for i in range(4):
      s,readval = parseEnv(readval)
      burrowertiles[1].append(s)
    ob = {'legalactions':
            legalactions,
            'turn':
            turn,
            'squad':
            squad,
            'hp':
            hp,
            'resist':
            resist,
            'perfect':
            perfect,
            'mission':
            mission,
            'progress':
            progress,
            'tilestates':
            tilestates,
            'occupants':
            occupants,
            'occhealth':
            occhealth,
            'occstatus':
            occstatus,
            'attackorder':
            attackorder,#source coord, target coord
            'burrowertiles':
            burrowertiles#locx, locy, hp, shielded
        }
    print(ob)
    print(score)


if __name__ == "__main__":
    main()

    
