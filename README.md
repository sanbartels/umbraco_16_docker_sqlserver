# umbraco_16_docker_sqlserver
 
Se crea una carpeta donde se almacenará el umbraco y el docker, en este caso 
`MyDockerProject`. 

Se crea un proyecto umbraco con soporte de docker con el comando 
`dotnet new umbraco -n MyDockerProject --add-docker`. 

Se agregan los archivos Compose con 
`dotnet new umbraco-compose -P "MyDockerProject"`. 

Se corré el docker con el siguiente comando 
`docker compose up`. 

Se puede ver el umbraco corriendo en la siguiente dirección [http://localhost:44372](http://localhost:44372). 

Al entrar se vería lo siguiente 

<img width="1920" height="1034" alt="imagen" src="https://github.com/user-attachments/assets/38488e6b-2575-4f90-81ac-411c940fa969" />.
