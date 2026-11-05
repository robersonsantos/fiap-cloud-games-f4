# Falas do Vídeo — Tech Challenge Fase 4 ("Pitch Final")

Texto que cada pessoa vai narrar, cena a cena, com a **marcação de tempo** no vídeo final.
O vídeo é gravado **sem áudio**; depois cada fala é encaixada na sua marcação.
Os comandos e o que aparece na tela de cada cena estão no [ROTEIRO-VIDEO-FASE4.md](ROTEIRO-VIDEO-FASE4.md).

## Marcações (vídeo final sincronizado com as falas gravadas)

- Cada cena dura **o tempo real da fala + 1 segundo** de pausa; a fala começa no início exato da cena.
- Vídeo final: **12:43** — `FIAP-Cloud-Games-Fase4-Grupo90.mp4`.
- **[TELA]** = o que aparece no vídeo naquele momento (não é para ler).

## Linha do tempo

| Cena | Quem fala | Início | Fim | Duração |
|---|---|---|---|---|
| — | *Abertura (sem fala)* | 0:00 | 0:02 | 3 s |
| C01 | Glayson | **0:03** | **1:09** | 67 s |
| C02 | Glayson | **1:09** | **1:51** | 42 s |
| C03 | Glayson | **1:51** | **2:24** | 33 s |
| C04 | Glayson | **2:24** | **2:52** | 28 s |
| C05 | Glayson | **2:52** | **3:31** | 39 s |
| C06 | Glayson | **3:31** | **4:15** | 44 s |
| C07 | Diogo | **4:15** | **5:18** | 63 s |
| C08 | Diogo | **5:18** | **5:49** | 32 s |
| C09 | Diogo | **5:49** | **6:17** | 28 s |
| C10 | Diogo | **6:17** | **6:37** | 20 s |
| C11 | Roberson | **6:37** | **7:14** | 37 s |
| C12 | Roberson | **7:14** | **7:52** | 38 s |
| C13 | Roberson | **7:52** | **8:23** | 31 s |
| C14 | Roberson | **8:23** | **9:00** | 36 s |
| C15 | Jhonatan | **8:59** | **9:33** | 34 s |
| C16 | Jhonatan | **9:33** | **10:11** | 37 s |
| C17 | Jhonatan | **10:11** | **10:48** | 37 s |
| C18 | Jhonatan | **10:48** | **11:14** | 26 s |
| C19 | Diogo | **11:14** | **11:59** | 45 s |
| C20 | Glayson | **11:59** | **12:39** | 39 s |
| — | *Tela final (sem fala)* | 12:39 | 12:43 | 4 s |

---

## Bloco 0 — Abertura

### C01 · Glayson · 0:03 a 1:09 (67 s)
**[TELA]** Slide/título "FIAP Cloud Games — Fase 4" → diagrama de arquitetura do README.

> Olá! Nós somos o Grupo 90: eu sou o Glayson, e comigo estão o Diogo, o Roberson e o Jhonatan.
> Este é o pitch final da FIAP Cloud Games.
>
> Na fase anterior, a plataforma ficou distribuída e observável, com API Gateway, mensageria e métricas.
> Nesta fase, o desafio foi resolver três gargalos para crescer de forma profissional: ter um ambiente
> de produção gerenciado na nuvem, eliminar o deploy manual e melhorar a performance com busca e cache.
>
> Escolhemos a AWS. A aplicação roda no Amazon EKS, que é o Kubernetes gerenciado da AWS. Todo push na
> branch main passa por uma pipeline no GitHub Actions até chegar em produção. Os dados ficam em bancos
> diferentes, cada um no que faz melhor: SQL Server, MongoDB, Redis e Amazon OpenSearch. E nenhuma senha
> está no código.
>
> Vamos mostrar tudo isso funcionando ao vivo.

---

## Bloco 1 — Infraestrutura na AWS

### C02 · Glayson · 1:09 a 1:51 (42 s)
**[TELA]** Console AWS → EKS → cluster `fcg` → abas *Overview*, *Compute* e *Add-ons*.

> Este é o nosso cluster no Amazon EKS, na região de Ohio, nos Estados Unidos, rodando o Kubernetes 1.36.
> Como o serviço é gerenciado, a AWS cuida do plano de controle: disponibilidade, atualizações e segurança
> do Kubernetes. Nós cuidamos só da aplicação.
>
> Na aba Compute está o node group, com dois nós t3.medium, onde os nossos pods rodam. Nos add-ons estão
> a rede, o DNS e o driver de discos EBS, que dá armazenamento persistente aos bancos.

### C03 · Glayson · 1:51 a 2:24 (33 s)
**[TELA]** ECR → repositórios `fcg/*` → abrir `fcg/catalog-api` → tags e resultado do scan.

> As imagens Docker ficam no Amazon ECR, que é o registry privado da AWS. Temos um repositório para cada
> microsserviço.
>
> Cada imagem recebe como tag o código do commit que a gerou, e as tags são imutáveis. Então sempre sabemos
> exatamente qual versão do código está em produção. O ECR também verifica cada imagem enviada em busca de
> vulnerabilidades conhecidas; esse resultado aparece aqui e também no resumo da pipeline.

### C04 · Glayson · 2:24 a 2:52 (28 s)
**[TELA]** EC2 → Load Balancers → o ELB do Kong. Depois Amazon OpenSearch → domínio `fcg-search`.

> A aplicação é acessada de fora por este Load Balancer. Ele foi criado automaticamente pelo Kubernetes
> quando publicamos o Kong, o nosso API Gateway, com um serviço do tipo LoadBalancer. Toda requisição
> entra por ele e passa pelo Kong, que valida o token JWT antes de encaminhar para os microsserviços.
>
> E este é o Amazon OpenSearch, o serviço gerenciado de busca que o Roberson vai demonstrar daqui a pouco.

### C05 · Glayson · 2:52 a 3:31 (39 s)
**[TELA]** Secrets Manager → `fcg/app` (detalhes do segredo; **os valores nunca aparecem**). Depois, VS Code com
`infra/terraform/secrets.tf` (senhas geradas por `random_password`) e `eks.tf`.

> Segurança: nenhuma string de conexão, chave ou senha está no código ou nos arquivos YAML.
>
> Todas as credenciais foram geradas automaticamente pelo Terraform e guardadas no AWS Secrets Manager.
> Ninguém do grupo digitou ou sequer conhece essas senhas. Na hora do deploy, a pipeline lê esse segredo
> e cria um Secret do Kubernetes, e os pods recebem os valores só em tempo de execução.
>
> E toda essa infraestrutura (rede, cluster, registry, OpenSearch e segredos) está descrita como código
> no Terraform. Para recriar o ambiente do zero, basta um comando.

### C06 · Glayson · 3:31 a 4:15 (44 s)
**[TELA]** Terminal: `kubectl get nodes`, `kubectl get pods -n fcg`, `kubectl get svc kong -n fcg`,
`kubectl get pvc -n fcg`, `GET /api/jogos` e a compra sem token retornando 401.

> No terminal, o mesmo ambiente visto pelo Kubernetes: os dois nós e todos os pods no namespace fcg,
> que são os quatro microsserviços, o Kong, os bancos, o RabbitMQ e o monitoramento. A UsersAPI e a
> CatalogAPI rodam com duas réplicas cada.
>
> O SQL Server e o MongoDB usam volumes persistentes em discos EBS, então os dados sobrevivem se um pod
> for reiniciado.
>
> Chamando a API pela internet, através do Load Balancer, a listagem de jogos responde normalmente. Já a
> compra sem token é barrada pelo Kong com 401, antes mesmo de chegar ao microsserviço.
>
> Agora o Diogo vai mostrar como uma alteração de código chega até aqui.

---

## Bloco 2 — CI/CD e Live Deploy

### C07 · Diogo · 4:15 a 5:18 (63 s)
**[TELA]** VS Code → `.github/workflows/_service-pipeline.yml` (rolar pelos 3 jobs).

> Obrigado, Glayson. Todo o ciclo de entrega é automatizado com GitHub Actions, e o código na branch
> main é sempre o que está em produção.
>
> A pipeline tem três etapas. Primeiro, Build e Test: compila a solução e roda os testes unitários. Se
> algum teste falhar, nada é publicado.
>
> Depois, a imagem Docker é gerada com a tag do commit, enviada para o ECR e verificada pelo scan de
> vulnerabilidades.
>
> Por último vem o deploy no EKS. Um detalhe de segurança: a pipeline usa um usuário da AWS exclusivo
> para deploy, com o mínimo de permissão: ele só publica imagens no ECR, acessa o cluster e lê os
> segredos da aplicação. A chave dele fica nos Secrets criptografados do GitHub, nunca no código.
>
> A pipeline cobre os quatro microsserviços, e cada um só é publicado quando o seu código muda.

### C08 · Diogo · 5:18 a 5:49 (32 s)
**[TELA]** `CatalogAPI/k8s/deployment.yaml`: `strategy`, `maxUnavailable: 0`, probes e `secretKeyRef`.

> E como atualizamos produção sem derrubar o sistema? Com o Rolling Update do Kubernetes.
>
> Com maxUnavailable zero, o Kubernetes primeiro sobe um pod com a versão nova e só desliga um pod antigo
> quando o novo responde no health check. A capacidade nunca diminui durante a troca.
>
> E se a versão nova não ficar saudável, a própria pipeline faz o rollback para a versão anterior
> automaticamente.

### C09 · Diogo · 5:49 a 6:17 (28 s)
**[TELA]** Tela dividida: (A) `kubectl get pods -w`; (B) loop chamando `/api/catalogo/versao`. Depois, VS Code
alterando a constante `Mensagem` em `VersaoController.cs`, e o terminal com `git commit` e `git push`.

> Agora o live deploy. À esquerda, os pods da CatalogAPI. À direita, um loop que consulta a API a cada
> meio segundo e mostra a mensagem da versão atual e qual pod respondeu.
>
> Vou alterar a mensagem no código: de "Fase 4 no Amazon EKS" para "Live Deploy do Grupo 94 funcionando".
> Commit... e push para a main.

### C10 · Diogo · 6:17 a 6:37 (20 s)
**[TELA]** GitHub → Actions → workflow `catalog-api` rodando (Build & Test em andamento).

> O push já disparou a pipeline da CatalogAPI, e os testes estão rodando. Esse processo leva alguns
> minutos, então, enquanto ele roda, o Roberson vai mostrar a busca avançada e depois eu volto para
> mostrarmos a nova versão chegando em produção.

---

## Bloco 3 — Busca avançada (OpenSearch)

### C11 · Roberson · 6:37 a 7:14 (37 s)
**[TELA]** Terminal: `/search?q=dragn` e `/search?q=kingdon`.

> Obrigado, Diogo. Com o aumento do tráfego, a busca simples no banco relacional virou um gargalo. Por
> isso, a busca agora é feita no Amazon OpenSearch, de forma totalmente independente do SQL Server.
>
> Vou buscar "dragn", digitado errado, sem o "o". Mesmo assim, o primeiro resultado é "Dragon Quest
> Online". E "kingdon", com "n" no final, encontra "Lost Kingdom". Isso é a fuzzy search, que tolera
> erros de digitação.

### C12 · Roberson · 7:14 a 7:52 (38 s)
**[TELA]** Terminal: `/search?q=acao` → coluna `relevancia`. Depois, VS Code em `OpenSearchJogoIndex.cs` (método `BuscarAsync`).

> Também tratamos acentos: buscar "acao", sem cedilha e sem til, encontra os jogos da categoria Ação.
>
> Repare na coluna de relevância: os resultados vêm ordenados pelo score do OpenSearch. No código, a
> consulta usa um multi_match com fuzziness automática. O nome do jogo pesa três vezes mais, a categoria
> duas vezes, e a descrição uma. Assim, um termo que aparece no nome fica acima de um que só aparece
> na descrição.

### C13 · Roberson · 7:52 a 8:23 (31 s)
**[TELA]** Terminal: criar "Galactic Racers" (POST como admin) → `/search?q=galatic` encontra o jogo.

> E como o índice se mantém atualizado? A sincronização está na CatalogAPI. Sempre que um jogo é criado
> ou editado no banco principal, o serviço atualiza o documento no OpenSearch, e quando o jogo é
> removido, ele sai do índice.
>
> Vou cadastrar um jogo novo, "Galactic Racers", e buscar logo em seguida, digitando errado, "galatic".
> Ele já aparece no resultado.

### C14 · Roberson · 8:23 a 9:00 (36 s)
**[TELA]** VS Code em `JogoService.cs` (`SincronizarIndiceAsync` / `RemoverDoIndiceAsync`) → `SearchIndexInitializer.cs`.

> Um cuidado de resiliência: o SQL Server continua sendo a fonte da verdade. Se o OpenSearch estiver
> fora do ar, o cadastro do jogo não é perdido; a falha fica registrada no log, e um endpoint de
> reindexação reconstrói o índice inteiro a partir do banco. Também, ao subir, a aplicação cria e
> popula o índice automaticamente se ele estiver vazio.
>
> Agora o Jhonatan vai falar de performance com cache e do NoSQL.

---

## Bloco 4 — Cache e NoSQL

### C15 · Jhonatan · 8:59 a 9:33 (34 s)
**[TELA]** Terminal: 6 chamadas a `/api/jogos` mostrando `X-Response-Time-Ms`.

> Obrigado, Roberson. Para reduzir a latência e a carga no banco, implementamos um cache distribuído com
> Redis, usando a abstração IDistributedCache do ASP.NET Core.
>
> Como o Roberson acabou de cadastrar um jogo, o cache foi invalidado. Por isso, a primeira chamada vai
> ao SQL Server, e as seguintes vêm direto do Redis. Vejam na tela a diferença no tempo de resposta da
> API entre a primeira chamada e as demais.

### C16 · Jhonatan · 9:33 a 10:11 (37 s)
**[TELA]** Logs da CatalogAPI com `CACHE MISS` / `CACHE HIT` → Grafana, painel de latência.

> Nos logs dá para ver exatamente isso: um CACHE MISS seguido de vários CACHE HIT.
>
> O cache usa a estratégia cache-aside, com expiração de cinco minutos. Quando o catálogo muda, uma
> versão é incrementada e todas as listagens em cache são invalidadas de uma vez, para o usuário nunca
> ver um preço desatualizado. E se o Redis falhar, a API continua funcionando, consultando direto o banco.
>
> No Grafana, o painel de latência mostra o efeito do cache nas requisições.

### C17 · Jhonatan · 10:11 a 10:48 (37 s)
**[TELA]** Terminal: POST de avaliação → GET `/api/jogos/3/avaliacoes` → consulta no MongoDB com `mongosh`.

> Agora o NoSQL. O sistema de avaliações de jogos é um dado flexível e de alto volume: cada avaliação tem
> nota, comentário opcional e pode ganhar novos campos no futuro. Esse tipo de dado se encaixa muito
> melhor em documentos do que em tabelas.
>
> Por isso, as avaliações ficam no MongoDB, acessado pelo driver oficial MongoDB.Driver. Vou avaliar um
> jogo... a API já devolve a média das notas. E aqui está o documento gravado diretamente no MongoDB.

### C18 · Jhonatan · 10:48 a 11:14 (26 s)
**[TELA]** VS Code em `AvaliacaoMongoRepository.cs` → diagrama (persistência poliglota).

> Essa é a persistência poliglota: cada dado no banco mais adequado. Jogos, compras e biblioteca
> precisam de transação e consistência, então ficam no SQL Server. Avaliações, flexíveis, ficam no
> MongoDB. Consultas repetidas ficam no Redis. E a busca textual fica no OpenSearch.
>
> Diogo, como está a pipeline?

---

## Bloco 2 (retorno) — Resultado do Live Deploy

### C19 · Diogo · 11:14 a 11:59 (45 s)
**[TELA]** GitHub Actions com os 3 jobs verdes + resumo (imagem e scan) → terminais: pods novos e a
mensagem nova no loop → `kubectl rollout history`.

> A pipeline terminou: testes aprovados, imagem nova no ECR com a tag do commit, scan concluído e o
> deploy feito no EKS.
>
> No terminal da esquerda, os pods novos subiram e os antigos foram encerrados um de cada vez. E no
> terminal da direita, a mensagem mudou para "Live Deploy do Grupo 94 funcionando", sem nenhum erro no
> meio. Os usuários não perceberam a troca.
>
> O histórico de rollout registra cada versão, com o commit que a gerou. Se fosse preciso, voltar à
> anterior seria um único comando, e a pipeline já faz isso sozinha em caso de falha.

---

## Bloco 5 — Encerramento

### C20 · Glayson · 11:59 a 12:39 (39 s)
**[TELA]** Diagrama de arquitetura → tabela "Requisitos → onde estão" do README.

> Recapitulando: a FIAP Cloud Games agora roda em um Kubernetes gerenciado na AWS, com imagens privadas
> no ECR e acesso externo por Load Balancer. A entrega é totalmente automatizada, do commit à produção,
> sem downtime e com rollback automático. Temos busca tolerante a erros no OpenSearch, cache com Redis,
> avaliações no MongoDB e nenhuma credencial no código.
>
> Toda a documentação, o Terraform, os manifestos Kubernetes e as pipelines estão no repositório.
>
> Obrigado pela atenção! Este foi o Grupo 90.
