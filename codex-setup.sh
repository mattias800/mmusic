set -euo pipefail
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt-get update

# Install .NET

apt-get install -y dotnet-sdk-9.0
cd MusicGQL
if compgen -G "*.sln" >/dev/null || compgen -G "*.csproj" >/dev/null; then
  echo "⛓  Found .NET project/solution file – running dotnet restore..."
  dotnet restore
else
  echo "⚠️  No .sln or .csproj file found. Skipping dotnet restore."
fi
cd ..

# Install Bun

apt-get install bun
cd Web
bun install
cd ..

