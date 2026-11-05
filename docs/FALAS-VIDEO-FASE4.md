# Falas do Vídeo — Tech Challenge Fase 4 ("Pitch Final")

Texto que cada pessoa vai narrar, cena a cena, com a **marcação de tempo** no vídeo final.
O vídeo é gravado **sem áudio**; depois cada fala é encaixada na sua marcação.
Os comandos e o que aparece na tela de cada cena estão no [ROTEIRO-VIDEO-FASE4.md](ROTEIRO-VIDEO-FASE4.md).

## Como usar as marcações

- Cada cena tem uma **janela** (início → fim). A fala começa **no segundo de início** e precisa terminar **até o fim**.
- As janelas foram calculadas com folga (leitura a ~115 palavras/min + 4 s). Lendo em ritmo normal (~130 palavras/min),
  sobram alguns segundos de silêncio no fim de cada cena, o que é normal.
- A cena seguinte começa no segundo logo após o fim da anterior (ex.: C01 termina em 1:25 e C02 começa em 1:26).
- O vídeo terá **17:04** de duração: cada cena gravada é cortada no tamanho exato da sua janela, então as
  marcações abaixo valem para o arquivo final.
- **[TELA]** = o que aparece no vídeo naquele momento (não é para ler).
- Gravação do áudio: **um arquivo por cena**, nomeado pela cena (ex.: `C03-glayson.m4a`), em lugar silencioso, com o
  celular a um palmo da boca. Se uma fala passar do fim da janela, me avise que eu ajusto a marcação.

## Linha do tempo

| Cena | Quem fala | Início | Fim | Janela | Fala estimada |
|---|---|---|---|---|---|
| — | *Vinheta de abertura (sem fala)* | 0:00 | 0:04 | 5 s | — |
| C01 | Glayson | **0:05** | **1:25** | 80 s | ~65 s (141 palavras) |
| C02 | Glayson | **1:26** | **2:11** | 45 s | ~36 s (77 palavras) |
| C03 | Glayson | **2:12** | **2:57** | 45 s | ~32 s (70 palavras) |
| C04 | Glayson | **2:58** | **3:38** | 40 s | ~32 s (69 palavras) |
| C05 | Glayson | **3:39** | **4:34** | 55 s | ~41 s (89 palavras) |
| C06 | Glayson | **4:35** | **5:45** | 70 s | ~50 s (108 palavras) |
| C07 | Diogo | **5:46** | **7:06** | 80 s | ~65 s (140 palavras) |
| C08 | Diogo | **7:07** | **7:47** | 40 s | ~30 s (66 palavras) |
| C09 | Diogo | **7:48** | **8:38** | 50 s | ~27 s (58 palavras) |
| C10 | Diogo | **8:39** | **9:09** | 30 s | ~19 s (41 palavras) |
| C11 | Roberson | **9:10** | **9:50** | 40 s | ~32 s (69 palavras) |
| C12 | Roberson | **9:51** | **10:36** | 45 s | ~33 s (71 palavras) |
| C13 | Roberson | **10:37** | **11:22** | 45 s | ~28 s (60 palavras) |
| C14 | Roberson | **11:23** | **12:08** | 45 s | ~34 s (73 palavras) |
| C15 | Jhonatan | **12:09** | **12:54** | 45 s | ~32 s (70 palavras) |
| C16 | Jhonatan | **12:55** | **13:45** | 50 s | ~36 s (79 palavras) |
| C17 | Jhonatan | **13:46** | **14:36** | 50 s | ~36 s (77 palavras) |
| C18 | Jhonatan | **14:37** | **15:07** | 30 s | ~22 s (47 palavras) |
| C19 | Diogo | **15:08** | **16:08** | 60 s | ~46 s (99 palavras) |
| C20 | Glayson | **16:09** | **16:59** | 50 s | ~36 s (79 palavras) |
| — | *Tela final (sem fala)* | 17:00 | 17:04 | 4 s | — |

| Pessoa | Cenas | Tempo total de fala |
|---|---|---|
| Glayson Sergio da Silva | C01, C02, C03, C04, C05, C06, C20 | ~4,9 min |
| Diogo Henrique Figueiredo Alves da Silva | C07, C08, C09, C10, C19 | ~3,1 min |
| Roberson Rabelo dos Santos | C11, C12, C13, C14 | ~2,1 min |
| Jhonatan Rodrigues Ferreira de Oliveira | C15, C16, C17, C18 | ~2,1 min |
| **Duração do vídeo** | | **17:04** |
---

## Bloco 0 — Abertura

### C01 · Glayson · 0:05 a 1:25 (80 s)
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

### C02 · Glayson · 1:26 a 2:11 (45 s)
**[TELA]** Console AWS → EKS → cluster `fcg` → abas *Overview*, *Compute* e *Add-ons*.

> Este é o nosso cluster no Amazon EKS, na região de Ohio, nos Estados Unidos, rodando o Kubernetes 1.36.
> Como o serviço é gerenciado, a AWS cuida do plano de controle: disponibilidade, atualizações e segurança
> do Kubernetes. Nós cuidamos só da aplicação.
>
> Na aba Compute está o node group, com dois nós t3.medium, onde os nossos pods rodam. Nos add-ons estão
> a rede, o DNS e o driver de discos EBS, que dá armazenamento persistente aos bancos.

### C03 · Glayson · 2:12 a 2:57 (45 s)
**[TELA]** ECR → repositórios `fcg/*` → abrir `fcg/catalog-api` → tags e resultado do scan.

> As imagens Docker ficam no Amazon ECR, que é o registry privado da AWS. Temos um repositório para cada
> microsserviço.
>
> Cada imagem recebe como tag o código do commit que a gerou, e as tags são imutáveis. Então sempre sabemos
> exatamente qual versão do código está em produção. O ECR também verifica cada imagem enviada em busca de
> vulnerabilidades conhecidas; esse resultado aparece aqui e também no resumo da pipeline.

### C04 · Glayson · 2:58 a 3:38 (40 s)
**[TELA]** EC2 → Load Balancers → o ELB do Kong. Depois Amazon OpenSearch → domínio `fcg-search`.

> A aplicação é acessada de fora por este Load Balancer. Ele foi criado automaticamente pelo Kubernetes
> quando publicamos o Kong, o nosso API Gateway, com um serviço do tipo LoadBalancer. Toda requisição
> entra por ele e passa pelo Kong, que valida o token JWT antes de encaminhar para os microsserviços.
>
> E este é o Amazon OpenSearch, o serviço gerenciado de busca que o Roberson vai demonstrar daqui a pouco.

### C05 · Glayson · 3:39 a 4:34 (55 s)
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

### C06 · Glayson · 4:35 a 5:45 (70 s)
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

### C07 · Diogo · 5:46 a 7:06 (80 s)
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

### C08 · Diogo · 7:07 a 7:47 (40 s)
**[TELA]** `CatalogAPI/k8s/deployment.yaml`: `strategy`, `maxUnavailable: 0`, probes e `secretKeyRef`.

> E como atualizamos produção sem derrubar o sistema? Com o Rolling Update do Kubernetes.
>
> Com maxUnavailable zero, o Kubernetes primeiro sobe um pod com a versão nova e só desliga um pod antigo
> quando o novo responde no health check. A capacidade nunca diminui durante a troca.
>
> E se a versão nova não ficar saudável, a própria pipeline faz o rollback para a versão anterior
> automaticamente.

### C09 · Diogo · 7:48 a 8:38 (50 s)
**[TELA]** Tela dividida: (A) `kubectl get pods -w`; (B) loop chamando `/api/catalogo/versao`. Depois, VS Code
alterando a constante `Mensagem` em `VersaoController.cs`, e o terminal com `git commit` e `git push`.

> Agora o live deploy. À esquerda, os pods da CatalogAPI. À direita, um loop que consulta a API a cada
> meio segundo e mostra a mensagem da versão atual e qual pod respondeu.
>
> Vou alterar a mensagem no código: de "Fase 4 no Amazon EKS" para "Live Deploy do Grupo 94 funcionando".
> Commit... e push para a main.

### C10 · Diogo · 8:39 a 9:09 (30 s)
**[TELA]** GitHub → Actions → workflow `catalog-api` rodando (Build & Test em andamento).

> O push já disparou a pipeline da CatalogAPI, e os testes estão rodando. Esse processo leva alguns
> minutos, então, enquanto ele roda, o Roberson vai mostrar a busca avançada e depois eu volto para
> mostrarmos a nova versão chegando em produção.

---

## Bloco 3 — Busca avançada (OpenSearch)

### C11 · Roberson · 9:10 a 9:50 (40 s)
**[TELA]** Terminal: `/search?q=dragn` e `/search?q=kingdon`.

> Obrigado, Diogo. Com o aumento do tráfego, a busca simples no banco relacional virou um gargalo. Por
> isso, a busca agora é feita no Amazon OpenSearch, de forma totalmente independente do SQL Server.
>
> Vou buscar "dragn", digitado errado, sem o "o". Mesmo assim, o primeiro resultado é "Dragon Quest
> Online". E "kingdon", com "n" no final, encontra "Lost Kingdom". Isso é a fuzzy search, que tolera
> erros de digitação.

### C12 · Roberson · 9:51 a 10:36 (45 s)
**[TELA]** Terminal: `/search?q=acao` → coluna `relevancia`. Depois, VS Code em `OpenSearchJogoIndex.cs` (método `BuscarAsync`).

> Também tratamos acentos: buscar "acao", sem cedilha e sem til, encontra os jogos da categoria Ação.
>
> Repare na coluna de relevância: os resultados vêm ordenados pelo score do OpenSearch. No código, a
> consulta usa um multi_match com fuzziness automática. O nome do jogo pesa três vezes mais, a categoria
> duas vezes, e a descrição uma. Assim, um termo que aparece no nome fica acima de um que só aparece
> na descrição.

### C13 · Roberson · 10:37 a 11:22 (45 s)
**[TELA]** Terminal: criar "Galactic Racers" (POST como admin) → `/search?q=galatic` encontra o jogo.

> E como o índice se mantém atualizado? A sincronização está na CatalogAPI. Sempre que um jogo é criado
> ou editado no banco principal, o serviço atualiza o documento no OpenSearch, e quando o jogo é
> removido, ele sai do índice.
>
> Vou cadastrar um jogo novo, "Galactic Racers", e buscar logo em seguida, digitando errado, "galatic".
> Ele já aparece no resultado.

### C14 · Roberson · 11:23 a 12:08 (45 s)
**[TELA]** VS Code em `JogoService.cs` (`SincronizarIndiceAsync` / `RemoverDoIndiceAsync`) → `SearchIndexInitializer.cs`.

> Um cuidado de resiliência: o SQL Server continua sendo a fonte da verdade. Se o OpenSearch estiver
> fora do ar, o cadastro do jogo não é perdido; a falha fica registrada no log, e um endpoint de
> reindexação reconstrói o índice inteiro a partir do banco. Também, ao subir, a aplicação cria e
> popula o índice automaticamente se ele estiver vazio.
>
> Agora o Jhonatan vai falar de performance com cache e do NoSQL.

---

## Bloco 4 — Cache e NoSQL

### C15 · Jhonatan · 12:09 a 12:54 (45 s)
**[TELA]** Terminal: 6 chamadas a `/api/jogos` mostrando `X-Response-Time-Ms`.

> Obrigado, Roberson. Para reduzir a latência e a carga no banco, implementamos um cache distribuído com
> Redis, usando a abstração IDistributedCache do ASP.NET Core.
>
> Como o Roberson acabou de cadastrar um jogo, o cache foi invalidado. Por isso, a primeira chamada vai
> ao SQL Server, e as seguintes vêm direto do Redis. Vejam na tela a diferença no tempo de resposta da
> API entre a primeira chamada e as demais.

### C16 · Jhonatan · 12:55 a 13:45 (50 s)
**[TELA]** Logs da CatalogAPI com `CACHE MISS` / `CACHE HIT` → Grafana, painel de latência.

> Nos logs dá para ver exatamente isso: um CACHE MISS seguido de vários CACHE HIT.
>
> O cache usa a estratégia cache-aside, com expiração de cinco minutos. Quando o catálogo muda, uma
> versão é incrementada e todas as listagens em cache são invalidadas de uma vez, para o usuário nunca
> ver um preço desatualizado. E se o Redis falhar, a API continua funcionando, consultando direto o banco.
>
> No Grafana, o painel de latência mostra o efeito do cache nas requisições.

### C17 · Jhonatan · 13:46 a 14:36 (50 s)
**[TELA]** Terminal: POST de avaliação → GET `/api/jogos/3/avaliacoes` → consulta no MongoDB com `mongosh`.

> Agora o NoSQL. O sistema de avaliações de jogos é um dado flexível e de alto volume: cada avaliação tem
> nota, comentário opcional e pode ganhar novos campos no futuro. Esse tipo de dado se encaixa muito
> melhor em documentos do que em tabelas.
>
> Por isso, as avaliações ficam no MongoDB, acessado pelo driver oficial MongoDB.Driver. Vou avaliar um
> jogo... a API já devolve a média das notas. E aqui está o documento gravado diretamente no MongoDB.

### C18 · Jhonatan · 14:37 a 15:07 (30 s)
**[TELA]** VS Code em `AvaliacaoMongoRepository.cs` → diagrama (persistência poliglota).

> Essa é a persistência poliglota: cada dado no banco mais adequado. Jogos, compras e biblioteca
> precisam de transação e consistência, então ficam no SQL Server. Avaliações, flexíveis, ficam no
> MongoDB. Consultas repetidas ficam no Redis. E a busca textual fica no OpenSearch.
>
> Diogo, como está a pipeline?

---

## Bloco 2 (retorno) — Resultado do Live Deploy

### C19 · Diogo · 15:08 a 16:08 (60 s)
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

### C20 · Glayson · 16:09 a 16:59 (50 s)
**[TELA]** Diagrama de arquitetura → tabela "Requisitos → onde estão" do README.

> Recapitulando: a FIAP Cloud Games agora roda em um Kubernetes gerenciado na AWS, com imagens privadas
> no ECR e acesso externo por Load Balancer. A entrega é totalmente automatizada, do commit à produção,
> sem downtime e com rollback automático. Temos busca tolerante a erros no OpenSearch, cache com Redis,
> avaliações no MongoDB e nenhuma credencial no código.
>
> Toda a documentação, o Terraform, os manifestos Kubernetes e as pipelines estão no repositório.
>
> Obrigado pela atenção! Este foi o Grupo 90.
