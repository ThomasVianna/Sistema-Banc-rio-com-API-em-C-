🏦 Sistema Bancário — API em C#

API desenvolvida em C# (.NET 9) que simula as operações básicas de um banco digital, permitindo consultar, cadastrar e movimentar contas de clientes.
Gerencia saldo, crédito disponível e histórico de transações (depósitos, saques e transferências).

🧩 Funcionalidades Principais
👤 Cadastro de Cliente

Permite criar novos clientes com os seguintes dados:

🪪 Nome completo

🧾 CPF

💵 Saldo inicial (decimal)

💳 Limite de crédito (decimal)

💰 Consulta de Conta

Retorna informações detalhadas de um cliente:

Nome

CPF

Saldo atual

Crédito disponível

Histórico de transações

🔁 Operações Bancárias

💸 Depósito: adiciona valor ao saldo

💵 Saque: subtrai do saldo (respeitando o limite de crédito)

🔄 Transferência: move valores entre contas distintas

Cada operação gera um registro no histórico contendo tipo, valor e data.

🌐 Endpoints da API
Método	Endpoint	Descrição
GET	/api/clientes	Lista todos os clientes
GET	/api/clientes/{id}	Exibe dados e saldo de um cliente
POST	/api/clientes	Cria um novo cliente
POST	/api/conta/deposito?id={id}&valor={valor}	Realiza um depósito
POST	/api/conta/saque?id={id}&valor={valor}	Realiza um saque
POST	/api/conta/transferir?origemId={id1}&destinoId={id2}&valor={valor}	Transfere valores entre contas
💻 Regras de Negócio

O saldo mínimo é igual ao limite de crédito negativo
Exemplo: limite = 500 → saldo mínimo = -500

Transferências afetam duas contas (origem e destino)

Nenhuma operação aceita valor ≤ 0

Todos os valores devem ser do tipo decimal (float e double não são aceitos)

⚙️ Recursos Extras (opcionais)

💾 Persistência com Entity Framework Core + SQLite

📘 Documentação interativa com Swagger (Swashbuckle)


📊 Endpoint para extrato mensal do cliente

🚀 Como Rodar o Projeto
# Clonar o repositório
git clone https://github.com/ThomasVianna/Sistema-Banc-rio-com-API-em-C-.git

# Acessar o diretório
cd SistemaBancarioAPI

# Restaurar as dependências
dotnet restore

# Executar o projeto
dotnet run


Depois, acesse no navegador:
👉 https://localhost:5151/swagger

🧠 Tecnologias Utilizadas

🧩 C# / .NET 9

🌐 ASP.NET Core Web API

🧰 Swashbuckle (Swagger UI)
