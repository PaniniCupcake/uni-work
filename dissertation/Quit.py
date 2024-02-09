import numpy as np
import main_write
import time
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

    main_write.write(str(2))
    time.sleep(1)
    main_write.write(str(0))
    print("Done")




if __name__ == "__main__":
    main()

    
