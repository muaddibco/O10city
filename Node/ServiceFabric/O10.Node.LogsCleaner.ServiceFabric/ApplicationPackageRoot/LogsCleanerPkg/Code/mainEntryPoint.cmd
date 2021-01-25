:loop
rmdir /s /q %FolderToDelete%
rmdir /s /q %FolderToDelete2%
rmdir /s /q %FolderToDelete3%
rmdir /s /q %FolderToDelete4%
ping -n %SleepInterval% 127.0.0.1>nul
goto loop
