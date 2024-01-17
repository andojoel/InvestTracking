import os

# Check if the folder migration exists
migrationFolderPath = './Migrations'

if os.path.isdir(migrationFolderPath) :
    os.system("rm -r ./Migrations")
    print("Migration folder deleted successfully.")

# Check if the database.db file exists
dbFilePath = './database.db'

if os.path.isfile(dbFilePath) :
    os.remove(dbFilePath)
    print("Database deleted successfully.")

if not os.path.isdir(migrationFolderPath) and not os.path.isfile(dbFilePath) :
    print("Init Enfity Framework migration...")
    os.system("dotnet ef migrations add InitialMigration")
    os.system("dotnet ef database update")
    os.system("dotnet build")