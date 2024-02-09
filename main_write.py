import time
import shared_memory

def write(s):
    shared_memory.write_string(str(s))
    