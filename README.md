# Full Medium Theory & Guide Link - [https://medium.com/@nakib.rahaman26/building-a-dockerized-o-1-user-lookup-api-in-net-using-redis-bloom-filters-09f0b083f6ea]
# User Guide
## 1. Configure Docker Desktop for Large Seeding
<div align="center">
<img width="2400" height="1133" alt="Docker Desktop Resource Settings"
src="https://github.com/user-attachments/assets/66496dfa-5fdd-4431-973c-8645de14e0c7" />
    <p><em>Resources → **Uncheck Resource Saver** </em></p>
</div> 
(Once seeding is completed, you may enable this again.)

## 2. Configure `.wslconfig`

### 2.1 Create / Edit `.wslconfig`
### 2.2 Add this configuration
```ini
[wsl2]
memory=6GB
processors=4
swap=8GB
```
### 2.3 Restart PC/Laptop to apply this configuration
if you are not in a position to restart then fully restart WSL (VERY IMPORTANT)
```
wsl --shutdown
```
After that, Restart Docker desktop. But I would suggest to Restart the PC/Laptop for surely configured new WSL settings.
## 3. Docker Build
first we need to open powershell where docker-compose exists then run the below command:
```
docker-compose up --build -d
```
<div align="center">
  <img src="https://github.com/user-attachments/assets/815bd509-f9cd-4c06-a8cd-49f261011e22" width="975" height="351" alt="Pulling SQL Server and Redis official images">
  <p><em>Pulling the SQL Server and Redis official images</em></p>
</div>
<div align="center">
<img width="975" height="464" alt="image" src="https://github.com/user-attachments/assets/7f276849-4ad9-4a8e-a1c6-4a06e67006c1" />
<p><em>Build the project following dockerfile</em></p>
</div>
<div align="center">
<img width="975" height="460" alt="image" src="https://github.com/user-attachments/assets/e80ab0bf-1be6-4b3a-81f4-f0da5de389d5" />
  <p><em>container started and volume set</em></p>
</div>  

## 4.Check running containers
```
docker ps
```
<div align="center">
<img width="975" height="128" alt="image" src="https://github.com/user-attachments/assets/c8eeddf0-524a-4afa-ad76-e1a80c3d9fc8" />
</div>

## 5.Now we check the 5000 port where bloom-filter-api running
<div align="center">
  <img width="2400" height="1097" alt="image" src="https://github.com/user-attachments/assets/9c882f3e-3918-4955-8675-eecb6c9596ae" />
  <p><i>OUR API RUNNING SUCCESSFULLY</i></p>
</div>

## 6.Migrations and DB persists check
<div align="center">
<img width="2400" height="2398" alt="image" src="https://github.com/user-attachments/assets/80513269-2cc6-4716-a5b0-5811c10b6a9b" />
  <p><i>DB IS CREATED BUT NO DATA AS EXPECTED</i></p>
</div>

## 7.Seed 2 Million Users
<div align="center">
   <img width="2400" height="2398" alt="image" src="https://github.com/user-attachments/assets/bc6c25ba-ceb1-4607-a24a-85f7a88b9273" />
  <p><i>Seeding 2 Million users Completed</i></p>
</div>

## 8.Extract first 100 users from DB
<div align="center">
   <img width="2400" height="3013" alt="image" src="https://github.com/user-attachments/assets/da34c426-765b-46ce-9ee8-9ed3690c77e0" />
  <p><i>Seeding 2 Million users Completed</i></p>
</div>

## 9.Check user existence via POSTMAN and measure the response time.
<div align="center">
  <img width="2400" height="1264" alt="image" src="https://github.com/user-attachments/assets/6f0ed0fa-e72c-4167-8fed-c2684345b88e" />
  <p><i>We check an existing users from 2 million users it gave us the check with DB query only in 519ms for checking response time we use postman tools instead of Swagger</i></p>
</div>

<div align="center">
   <img width="2400" height="1262" alt="image" src="https://github.com/user-attachments/assets/4ce303a2-e9b3-40cb-9815-8c4b79fd99dc" />
  <p><i>Now we check an unregistered user that is not part of that 2 million users data.</i></p>
</div>

So, we now confirmed that it just take only 11ms to confirm that a user is not existed with the help of bloomfilter and redis super fast that is good for production.
## 10.Check if Bloom Filter data persists after Redis container stops or is removed [as we add volumes]:
<div align="center">
  <img width="2400" height="1252" alt="image" src="https://github.com/user-attachments/assets/d0e06373-9808-44ba-bb03-4a43434a263c" />
  <p><i>Delete All the running containers</i></p>
</div>

<div align="center">
<img width="2400" height="1243" alt="image" src="https://github.com/user-attachments/assets/a8f4c601-6808-4829-be0e-dbc7e1df4228" />
  <p><i>Stop all containers, rebuild with Docker Compose, and observe faster build due to cache.</i></p>
</div>

<div align="center">
  <img width="2400" height="1258" alt="image" src="https://github.com/user-attachments/assets/e481eb42-7976-431f-b3ea-85ff083e15ee" />
  <p><i>After bringing containers up without deleting volumes, we see that previously seeded data persists even after the container was stopped.</i></p>
</div>

<div align="center">
<img width="2400" height="1258" alt="image" src="https://github.com/user-attachments/assets/cc39cf17-919a-4cf4-bf58-6f9f156fe2df" />
  <p><i>Now again for same request look at the response time just wow 340ms to 56ms super fast</i></p>
</div>

<div align="center">
<img width="2400" height="1256" alt="image" src="https://github.com/user-attachments/assets/f48c43fa-6643-4b43-a92e-1b310bfde5ea" />
  <p><i>Now again for same request look at the response time just wow 56ms to 13ms super ultra fast</i></p>
</div>

<div align="center">
<img width="975" height="650" alt="image" src="https://github.com/user-attachments/assets/3e369a23-02ca-4aed-84bd-4dfa234f2b35" />
</div>

## 10.Access database inside Docker container via CLI
### 10.1-Identify SQL Server DB container
<div align="center">
<img width="975" height="95" alt="image" src="https://github.com/user-attachments/assets/af5402d2-69a0-4553-8ce6-4043ad7ee36c" />
</div>

### 10.2-login in sql server container
```
sqlcmd -S localhost,1433 -U sa -P "YourStrong@Password123"
SELECT name FROM sys.databases;
go
```

<div align="center">
<img width="975" height="179" alt="image" src="https://github.com/user-attachments/assets/721ea23b-ac27-466a-b0d6-db8b5068109a" />
  <p><i>we see that we sucessfully enter into the DB and we read all the db names existed on that sql server container</i></p>
</div>

### 10.3-Change the DB
```
USE BloomFilterTest;
go
```
<div align="center">
<img width="902" height="61" alt="image" src="https://github.com/user-attachments/assets/cbd63e62-7241-451b-a098-fe1ebfd2e42b" />
  <p><i>Select the desired DB because we have to see the data inside this specific DB [BloomFilterTest]</i></p>
</div>

### 10.4-Find all the available tables in that 'BloomFilterTest' DB
```
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
go
```
<div align="center">
  <img width="975" height="144" alt="image" src="https://github.com/user-attachments/assets/5d815a91-0df3-4b3b-aa09-d8d16e00eb3f" />
  <p><i>we finally see the Database Tables</i></p>
</div>

### 10.5-Count the actual users amount
```
select count(*) as TotalUsers from Users;
go
```
<div align="center">
  <img width="2400" height="394" alt="image" src="https://github.com/user-attachments/assets/004281cf-59d8-4d64-8b0e-a5a2287951fa" />
  <p><i>we see that we are actually working with 2 Million Users</i></p>
</div>

### 10.6-Now check the 'user97470@example.com' exists or not in DB SQL container
```
select top 1 * from Users where Email = 'user97470@example.com';
go
```
<div align="center">
<img width="2400" height="617" alt="image" src="https://github.com/user-attachments/assets/6964bf5d-4b0a-4b5a-a153-82490fd01210" />
  <p><i>Hurrah we found it</i></p>
</div>

## 11.Final Verdict
- Validated locally with millions of users to prove correctness and performance
- Same design scales to billions in production without logic changes
- Predictable and memory-efficient data structure
- Significant reduction in database load and latency
