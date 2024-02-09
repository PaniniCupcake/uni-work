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
    return ss,s, ns



def main():
    #print("Wow We actually got  this far")

    readval = ""
    f = open("../ActData.txt","r")
    readval = f.read()
    f.close()
    print(len(readval))
    f = open("../EnvData.txt","r")
    readval = f.read()
    f.close()
    count = 0
    score = 0
    readval = readval[3:]
    ob = []
    s = ""
    s,readval, _ = parseEnv(readval)
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
    print("0.0")
    for i in range(1000):
        s,readval, ns = parseEnv(readval)
        if(ns > 0):
           print(readval)
           print(len(ob))
        if s == '':
            break
        ob.append(s);
    print("Length!")
    print(len(ob))
    print(ob)

    

if __name__ == "__main__":
    main()
