# HttpsCert Generator for Windows

This tool easily creates HTTPS certificates (SSL) for testing.

Based on [BouncyCastle](https://github.com/bcgit/bc-csharp).

## Command example:
+ Normal:    (You will get `"www.example.com.key"` and `"www.example.com.crt"`.)

    ```.\httpscert -d www.example.com```

    ---

+ PFX:    (You will get `"www.example.com.key"`、`"www.example.com.crt"` and `"www.example.com.pfx"`.)

    ```.\httpcert -d www.example.com -p password```

    ---
    

+ Validity period:    (Valid for 3 years)
    
    ```.\httpscert -d www.example.com -y 3```
   
    ---
  
## License
[MIT](https://github.com/lalakii/HttpsCert/blob/master/LICENSE)