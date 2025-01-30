# HttpsCert for Windows

This tool easily creates HTTPS certificates for testing.

depends on [BouncyCastle](https://github.com/bcgit/bc-csharp)

## Command example:
+ Normal:

    ```.\httpscert -d www.example.com```

    ---

    You will get `"www.example.com.key"` and `"www.example.com.crt"`.

+ Pfx

    ```.\httpcert -d www.example.com -p password```

    ---
    
    You will get `"www.example.com.key"`、`"www.example.com.crt"` and `"www.example.com.pfx"`.

## License
[MIT](https://github.com/lalakii/HttpsCert/blob/master/LICENSE)