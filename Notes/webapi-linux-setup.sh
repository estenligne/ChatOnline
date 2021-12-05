# Open the Terminal window

# Make this script executable:
# chmod a+x webapi-linux-setup.sh

# Execute this script:
# ./webapi-linux-setup.sh

CLEAR='\033[0m' # No Color
RED='\033[0;31m'
GREEN='\033[0;32m'
SEPARATOR='echo -e "$GREEN------------------------------$CLEAR"'

# Prepare to install dotnet sdk and runtime:
# See https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

eval $SEPARATOR
sudo apt update
eval $SEPARATOR

# Install apt-transport-https:
sudo apt install -y apt-transport-https

# Install dotnet sdk and runtime:
sudo apt install -y dotnet-sdk-5.0 aspnetcore-runtime-5.0

# Checks:
which dotnet
dotnet --list-sdks
dotnet --list-runtimes

# Install dotnet-ef:
dotnet tool install --global dotnet-ef

eval $SEPARATOR

# Get the GitHub repository:
if test -d ChatOnline; then
	echo "ChatOnline directory already exists.";
else
	echo "ChatOnline directory does not exist.";
	git clone https://github.com/estenligne/ChatOnline
fi

# cd to the WebAPI folder:
cd ChatOnline/WebAPI/

eval $SEPARATOR

echo -e "Install MySQL $RED(Optional since SQLite is used by default)$CLEAR"
sudo apt install mysql-server mysql-client libmysqlclient-dev

# Setup MySQL database and user:
sudo mysql
# mysql> SHOW DATABASES;
# mysql> CREATE DATABASE `ChatOnline`;
# mysql> CREATE USER 'username'@'localhost' IDENTIFIED BY 'password';
# mysql> GRANT ALL PRIVILEGES ON `ChatOnline`.* TO 'username'@'localhost';
# mysql> FLUSH PRIVILEGES;
# mysql> exit

eval $SEPARATOR

# Restore dependencies and project-specific tools that are specified in the project file:
dotnet restore
eval $SEPARATOR

# Clean project (optional)
dotnet clean
eval $SEPARATOR

# Build project:
dotnet build
eval $SEPARATOR

# Run project (Press Ctrl+C to shut down)
dotnet run --no-build
eval $SEPARATOR

# Open the browser and enter the address:
# http://localhost:44363/swagger/index.html

# Open Visual Studio Code using the WebAPI workspace:
code WebAPI.code-workspace

# Inside VSCode, click on WebAPI.csproj. This will cause it to propose extensions to install. In particular, install: C# for Visual Studio Code (powered by OmniSharp).

