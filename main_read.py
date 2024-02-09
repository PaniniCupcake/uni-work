import shared_memory

def read():
    return shared_memory.read_string()
    
if __name__ == '__main__':
    print(read())