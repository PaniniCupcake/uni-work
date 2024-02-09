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
    send_ver = 1
    receive_ver = 0
    r = str(random.randrange(0,247))
    print("First write")
    print(r)
    main_write.write(str(send_ver) + r)
    f = open("../EnvData.txt", "w")
    random.seed()
    f.write("1")
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
            if(r == '246'):
              break
            continue
        f.close()
        count += 1
        #print(count)
    readval = readval[1:]
    ob = []
    s = ""
    score = 0
    s,readval = parseEnv(readval)
    score += int(s) * 20000000
    s,readval = parseEnv(readval)
    score += int(s) * 1000000
    s,readval = parseEnv(readval)
    score += int(s) * 1000
    s,readval = parseEnv(readval)
    score += int(s) * 100
    s,readval = parseEnv(readval)
    score += int(s)
    s,readval = parseEnv(readval)
    score += int(s)
    ob = []
    for i in range(782):
        s,readval = parseEnv(readval)
        ob.append(s);
    print("Length!")
    counter = 0
    for i in ob:
        if i == '':
            counter +=1
    print(counter)
    print(len(ob))
    print(ob)

    
    #print(ob)
    #print(score)

if __name__ == "__main__":
    main()

    
