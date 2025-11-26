import os

def list_scenes_contents():
    base_path = os.getcwd()
    scenes_path = os.path.join(base_path, "assets", "scenes")

    if not os.path.exists(scenes_path):
        print("No 'assets/scenes' folder found.")
        return

    print("assets/scenes/")
    for item in os.listdir(scenes_path):
        full_path = os.path.join(scenes_path, item)

        if os.path.isdir(full_path):
            print(f"├── {item}/")
        else:
            print(f"├── {item}")


if __name__ == "__main__":
    list_scenes_contents()
