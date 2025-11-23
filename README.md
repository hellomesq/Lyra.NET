# LYRA
A Lyra é uma aplicação que atua como um mentor digital de carreira, guiando o usuário por trilhas personalizadas de aprendizado e desenvolvimento profissional, alinhadas aos Objetivos de Desenvolvimento Sustentável (ODS) e às tendências do futuro do trabalho. A solução permite que os usuários descubram suas habilidades, interesses e potenciais carreiras de alto crescimento até 2030, oferecendo uma visão clara de como suas escolhas impactam o futuro.

A API é RESTful, construída em ASP.NET Core, utilizando Entity Framework Core para conexão com Oracle, e possui documentação automática com Swagger. Todas as rotas implementam HATEOAS e suporte a paginação.

## Integrantes

- Hellen Marinho Cordeiro RM 558841
- Heloisa Alves de Mesquita RM 559145

## Justificativa da Arquitetura 

A Lyra busca oferecer uma experiência de aprendizado personalizada, permitindo que o usuário:

- Cadastre-se e gerencie seu perfil profissional.
- Visualize e crie os títulos e descrições das trilhas de desenvolvimento alinhadas ao seu perfil e interesses.
- Acompanhe progresso por meio de listagens paginadas de trilhas concluídas.
- Tenha acesso a links HATEOAS em cada recurso, indicando operações disponíveis (GET, POST, PUT, DELETE).

Essa arquitetura permite manter escalabilidade, clareza e rastreabilidade, facilitando futuras integrações e melhorias.

## Como rodar os testes
A solução possui dois tipos de testes:
- Unitários: testam classes e métodos isoladamente, sem depender de banco de dados ou servidor HTTP.
- Integração: simulam chamadas HTTP para a API usando WebApplicationFactory, testando fluxo completo.

1. Dentro do terminal, execute na raiz do projeto para rodar todos os testes
   ```bash
   dotnet test
   ```
2. Para rodar apenas os testes unitários
   ```bash
   
   ```
3. Para rodar apenas os testes de integração
   ```bash
   
   ```
Exemplos de testes implementados:


## Como rodar a API

1. Clone o repositório:
   ```bash
   git clone https://github.com/hellomesq/Lyra
   cd Lyra
   ```

2. Configure a connection string do Oracle no arquivo `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "OracleConnection": "User Id=seu_usuario;Password=sua_senha;Data Source=seu_host:porta/seu_servico"
   }
   ```

3. Crie o banco de dados e aplique as migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Execute a aplicação:
   ```bash
   dotnet run
   ```

5. Acesse a documentação Swagger para explorar e testar as rotas:
   ```
   http://localhost:5209/swagger/index.html
   ```

## Fluxo de uso 

#### Cadastro de Usuário
```
POST /api/v1/User
{
  "name": "João Silva",
  "email": "joao@email.com",
  "password": "senha123",
  "experience_Level": "Junior"
}
```

#### Consulta de usuário
```
GET /api/v1/User/{id}
GET /api/v1/User/by-email?email=joao@email.com
```

#### Listagem de Trilhas de um Usuário
```
GET /api/v1/Carreira/{userId}?page=1&pageSize=10
```

#### Concluir uma Trilha
```
POST /api/v1/Carreira
{
  "UserId": 1,
  "Trilha": "Front-end React",
  "Descricao": "Aprender componentes, hooks e roteamento"
}
```

#### Atualizar uma Trilha
```
PUT /api/v1/Carreira/{id}
{
  "Trilha": "Front-end React Avançado",
  "Descricao": "Hooks, Redux e testes"
}
```

#### Deletar uma Trilha
```
DELETE /api/v1/Carreira/{id}
```

## HATEOAS
Cada recurso retornado inclui links que indicam operações relacionadas, como:
- self → link para o recurso atual
- update → link para atualizar o recurso
- delete → link para remover o recurso
- list → link para listar todos os recursos relacionados
A listagem de trilhas também fornece links de paginação: next e prev.
  
## Endpoints 


