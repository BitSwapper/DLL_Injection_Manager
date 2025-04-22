A simple winforms based injector which keeps a history of recently used dlls, and remembers the last dll and process used on application start.
It monitors the status of the DLL's and the last injected process to help prevent double injection errors, etc.

This simply uses LoadLibraryA and CreateRemoteThread as the injection method.

I would advise against using this anywhere with anticheat, it's just made to be highly functional for my use case really :p

![image](https://github.com/user-attachments/assets/12be1328-eefd-44ba-b731-59197188036e)
![image](https://github.com/user-attachments/assets/1150ff5e-5e1c-4b9d-b955-d0aa85013155)
![image](https://github.com/user-attachments/assets/8cb485fe-9fd1-47d7-adf9-9156ba676a69)
![image](https://github.com/user-attachments/assets/c0e78616-acd6-4cbd-b031-3441f353bf1e)
![image](https://github.com/user-attachments/assets/1ab953f8-e5ad-42f3-9e7a-369d30dbed4c)

