A simple winforms based injector which keeps a history of recently used dlls, and remembers the last dll and process used on application start.

libraries Injector32 and Injector64 can be used independantly if desired.
(i.e. just include Injector32.dll and InjectorCommon.dll in any project) 

The winform monitors the status of the DLL's and the last injected process to help prevent double injection errors, etc.

This simply uses LoadLibraryA and CreateRemoteThread as the injection method.

I would advise against using this anywhere with anticheat, it's just made to be highly functional for my use case really :p

The GUI bitness does not matter, it can inject into either 32 or 64 bit processes (thanks to the middleman exes)

![image](https://github.com/user-attachments/assets/12be1328-eefd-44ba-b731-59197188036e)
![image](https://github.com/user-attachments/assets/1150ff5e-5e1c-4b9d-b955-d0aa85013155)
![image](https://github.com/user-attachments/assets/8cb485fe-9fd1-47d7-adf9-9156ba676a69)
![image](https://github.com/user-attachments/assets/a8ffd096-b692-48ea-bfa8-c617be664a2e)
![image](https://github.com/user-attachments/assets/63554a09-33f1-401b-9c24-e0239e2b9a0b)


