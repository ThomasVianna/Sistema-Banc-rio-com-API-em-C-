🏦 Sistema Bancário com API em C#

API desenvolvida em C# (.NET) que simula as operações básicas de um banco digital.
Permite consultar, adicionar e atualizar informações financeiras de clientes, incluindo saldo, crédito disponível e movimentações (depósitos, saques e transferências).

🧩 Funcionalidades Principais
👤 Cadastro de Cliente

Criação de novos clientes com os seguintes dados:

Nome completo

CPF

Saldo inicial (decimal)

Limite de crédito (decimal)

💰 Consulta de Conta

Retorna informações detalhadas de um cliente:

Nome

CPF

Saldo atual

Crédito disponível

Histórico de transações

🔁 Operações Bancárias

Depósito: adiciona valor ao saldo.

Saque: subtrai do saldo (respeitando o limite de crédito).

Transferência: move valores entre contas diferentes.

Cada operação gera um registro no histórico de transações (tipo, valor e data).

🌐 Endpoints da API
Método	Endpoint	Descrição
GET	/api/clientes	Lista todos os clientes
GET	/api/clientes/{id}	Exibe dados e saldo de um cliente
POST	/api/clientes	Cria novo cliente
POST	/api/conta/deposito?id={id}&valor={valor}	Realiza um depósito
POST	/api/conta/saque?id={id}&valor={valor}	Realiza um saque
POST	/api/conta/transferir?origemId={id1}&destinoId={id2}&valor={valor}	Transfere entre contas
💻 Regras de Negócio

O saldo não pode ser menor que o limite de crédito negativo.
Exemplo: limite = 500 → saldo mínimo = -500.

As transferências afetam duas contas (origem e destino).

Nenhuma operação aceita valor menor ou igual a zero.

Todos os valores devem ser do tipo decimal (nunca float ou double).

⚙️ Recursos Extras (opcionais)

💾 Persistência com Entity Framework Core + SQLite

📘 Documentação interativa com Swagger

🔐 Autenticação via JWT

📊 Endpoint para extrato mensal do cliente

🚀 Como Rodar o Projeto

Clona o repositório:

git clone https://github.com/usuario/SistemaBancarioAPI.git


Acessa o diretório:

cd SistemaBancarioAPI


Restaura as dependências:

dotnet restore


Executa:

dotnet run


Abre no navegador:

https://localhost:5001/swagger

🧠 Tecnologias Utilizadas

C# / .NET 8

ASP.NET Core Web API

Swashbuckle (Swagger UI)

(Opcional) Entity Framework Core + SQLite