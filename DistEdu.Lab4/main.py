from lab4 import lab4_main
from lab5 import lab5_main
from common import common_main
from utils import constants

if __name__ == "__main__":
    n = ''
    paths_keys = [
        "dictionary_path",
        "double_dictionary_path",
        "coordinate_index_path",
        "double_index_path",
        "gramm_index_path",
        "shift_index_path",
        "dir_path",
        "index_path",
    ]
    paths = {}
    if len(n):
        for path in paths_keys:
            paths[path] = constants[path]
    else:
        for path in paths_keys:
            paths[path] = constants[path]
    try:
        task_num = int(input("Input the task number..."))
    except Exception as e:
        print("Exception: ", e)
    else:
        if task_num == 1:
            common_main(paths["dictionary_path"], paths["dir_path"])
        elif task_num == 4:
            lab4_main(
                paths["dictionary_path"],
                paths["dir_path"],
                paths["gramm_index_path"],
                paths["shift_index_path"],
            )
        elif task_num == 5:
            lab5_main(paths["dictionary_path"], paths["dir_path"])

        else:
            print("No such task.")