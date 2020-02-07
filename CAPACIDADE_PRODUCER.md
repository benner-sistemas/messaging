# RabbitMQ
Foram feitos os seguintes testes no RabbitMQ:
20 sistemas origem mandando 300 mensagens cada.
1 producer em container enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 628 bytes
média de enifleiramento: 100 msgs/s

20 sistemas origem mandando 300 mensagens cada.
1 producer em container enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 10 kbytes
média de enifleiramento: 85 msgs/s

20 sistemas origem mandando 300 mensagens cada.
5 producers em container (swarm) enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 628 bytes
média de enifleiramento: 140 msgs/s

20 sistemas origem mandando 300 mensagens cada.
5 producers em container (swarm) enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 10 kbytes
média de enifleiramento: 115 msgs/s


# ActiveMQ
Foram feitos os seguintes testes no ActiveMQ:
20 sistemas origem mandando 300 mensagens cada.
1 producer em container enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 628 bytes
média de enifleiramento: 22 msgs/s

20 sistemas origem mandando 300 mensagens cada.
1 producer em container enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 10 kbytes
média de enifleiramento: 24 msgs/s


20 sistemas origem mandando 300 mensagens cada.
5 producers em container (swarm) enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 628 bytes
média de enifleiramento: 24 msgs/s

20 sistemas origem mandando 300 mensagens cada.
5 producers em container (swarm) enfileirando as mensagens enviadas pelos sistemas origem.
tamanho mensagem: 10 kbytes
média de enifleiramento: 21 msgs/s