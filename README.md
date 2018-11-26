# Cryptographic transformations
Windows Forms application allows to encrypt or decrypt files, using Advansed Encryption Standard (AES) algorithm. Application uses .NET library "System.Security.Cryptography" for cryptotransform initiation. it is possible to choose Block Chaining Mode (default is CBC), Key Size (default is 128)
## Prerequisites & Installing
The application can be run on Windows 10, Windows 7, Windows XP with .NET runtime installed.
This application is portable so it doesn't need instalation.
## Using the application
Press the button "Choose file...". In File Open Dialog choose a file you want to process (encrypt or decrypt).
Enter password (at least 8 latin letters and / or digits).
Choose Block chaining mode and Key size (if not default values are used).
Press "Encrypt!" button if you want to encrypt the file. New encrypted file will be craeted in current directory, it's name will be modified by adding ".aes" extension, so original extension becomes a part of file name ("myfile.mp3" becomes "myfile.mp3.aes"). This allows to restore original file name while decryption.
### or
Press "Decrypt" button to decrypt file; however decryption is possible for encrypted files with ".aes" extension. If password entered for decryption is not identical to one that was used for encryption Error message "Wrong password" appears.
Decrypted file will be placed in current directory with it's original name before encryption. If a file with such name exists it will be rewrited.
## Testing the application
Simple way to test this application is to encrypt any file and then decrypt it, then check identity of original and decrypted files.
## License
This project is licensed under the MIT License
