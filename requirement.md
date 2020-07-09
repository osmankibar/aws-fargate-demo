### Preinstallation

* aws-cli
* eksctl
* kubectl
* helm


Create EKS cluster

``` bash
eksctl create cluster \
--name demo \
--version 1.16 \
--tags environment=demo \
--region eu-central-1 \
--nodegroup-name standard-workers \
--node-type m5.large \
--nodes 1 \
--nodes-min 1 \
--nodes-max 1 \
--managed \
--asg-access \
--node-volume-size=80 \
--external-dns-access \
--verbose=5 \
--fargate 

```


Taint First Node

``` bash
kubectl taint nodes $(kubectl get node -o jsonpath='{.items[0].metadata.name}') task=demo:NoSchedule

```

Create Node Group

``` bash

eksctl create nodegroup \
--cluster demo \
--name workers \
--region eu-central-1 \
--node-type t3.medium \
--nodes 1 \
--nodes-min 1 \
--nodes-max 10 \
--managed \
--asg-access \
--node-volume-size=20 \
--verbose=5 

```



Create Dashboard

``` bash

kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.3/aio/deploy/recommended.yaml

kubectl apply -f admin-user.yaml

```

Get Token

``` bash

kubectl -n kubernetes-dashboard describe secret $(kubectl -n kubernetes-dashboard get secret | grep admin-user | awk '{print $1}')

```

Main Server Port Forward 

```
kubectl port-forward -n demo svc/main-service 9801:80

```

Update K8 Url environment
