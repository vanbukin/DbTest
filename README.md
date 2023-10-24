Create volume
```shell
docker volume create --name pg-test
```

Run DB container

```shell
 docker run --rm -it  --mount source=pg-test,target=/var/lib/postgresql/data -it -e POSTGRES_PASSWORD=postgres -e POSTGRES_USER=postgres -e PGDATA=/var/lib/postgresql/data/pgdata -p 5432:5432 --cpus 2 -m 8G --shm-size=2G  postgres:16
```

Remove volume
```shell
docker volume rm pg-test
```