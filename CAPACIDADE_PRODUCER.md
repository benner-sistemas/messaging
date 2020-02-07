# RabbitMQ
Foram feitos os seguintes testes no RabbitMQ:

***

20 sistemas origem mandando 300 mensagens cada. 

1 producer em contêiner enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 628 bytes

Média de enfileiramento: 100 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

1 producer em contêiner enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 10 kbytes

Média de enfileiramento: 85 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

5 producers em contêiner (swarm) enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 628 bytes

Média de enfileiramento: 140 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

5 producers em contêiner (swarm) enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 10 kbytes

Média de enfileiramento: 115 msgs/s

***

# ActiveMQ
Foram feitos os seguintes testes no ActiveMQ:

***

20 sistemas origem mandando 300 mensagens cada.

1 producer em contêiner enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 628 bytes

Média de enfileiramento: 22 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

1 producer em contêiner enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 10 kbytes

Média de enfileiramento: 24 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

5 producers em contêiner (swarm) enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 628 bytes

Média de enfileiramento: 24 msgs/s

***

20 sistemas origem mandando 300 mensagens cada.

5 producers em contêiner (swarm) enfileirando as mensagens enviadas pelos sistemas origem.

Tamanho mensagem: 10 kbytes

Média de enfileiramento: 21 msgs/s
