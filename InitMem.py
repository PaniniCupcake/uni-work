import numpy as np
import main_write

def main():
    main_write.write(0)
    f = open("../EnvData.txt", "w")
    f.write("1")
    f.close()


if __name__ == "__main__":
    main()