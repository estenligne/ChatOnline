# Open the Terminal window

# Make this script executable:
# chmod a+x webapi-linux-setup.sh

# Execute this script:
# ./webapi-linux-setup.sh

# Install dotnet-sdk:
sudo snap install dotnet-sdk --classic --channel=5.0

# Install dotnet-runtime:
sudo snap install dotnet-runtime-50 --classic

# Install dotnet-ef:
dotnet tool install --global dotnet-ef

# Get GitHub repository:
git clone https://github.com/estenligne/ChatOnline

# cd to the WebAPI folder:
cd ChatOnline/WebAPI/

# Install MySQL:
sudo apt install mysql-server mysql-client libmysqlclient-dev

# Setup MySQL database and user:
sudo mysql
mysql> SHOW DATABASES;
mysql> CREATE DATABASE `ChatOnline`;
mysql> CREATE USER 'username'@'localhost' IDENTIFIED BY 'password';
mysql> GRANT ALL PRIVILEGES ON `ChatOnline`.* TO 'username'@'localhost';
mysql> FLUSH PRIVILEGES;
mysql> exit

# Restore dependencies and project-specific tools that are specified in the project file:
dotnet restore

# Build and run the project:
dotnet clean
dotnet build
dotnet run --no-build

# Go to the browser and enter the address:
# http://localhost:5000/swagger/index.html

# Open Visual Studio Code using current directory:
code .

# Inside VSCode, click on WebAPI.csproj. This will cause Visual Studio Code to propose extensions to install. In particular, install the extension: C# for Visual Studio Code (powered by OmniSharp).

