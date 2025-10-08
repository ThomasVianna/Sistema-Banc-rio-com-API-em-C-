const form = document.getElementById('transacaoForm');
const clienteForm = document.getElementById('clienteForm');
const resultadoDiv = document.getElementById('resultado');
const tipoSelect = document.getElementById('tipo');
const valorInput = document.getElementById('valor');
const clienteSelect = document.getElementById('clienteId');

// Função para exibir mensagens ao usuário
const mostrarMensagem = (mensagem, isErro = false) => {
    resultadoDiv.innerText = mensagem;
    resultadoDiv.classList.toggle('erro', isErro);
    resultadoDiv.classList.toggle('sucesso', !isErro);
};

// Função para validar entrada do formulário de transação
const validarEntrada = (clienteId, tipo, valor) => {
    if (!clienteId) {
        return 'Por favor, selecione um cliente.';
    }
    if (!tipo) {
        return 'Por favor, selecione um tipo de transação.';
    }
    if (isNaN(valor) || valor <= 0) {
        return 'Por favor, insira um valor numérico maior que zero.';
    }
    if (!/^\d+(\.\d{1,2})?$/.test(valor)) {
        return 'O valor deve ter no máximo duas casas decimais.';
    }
    return null;
};

// Função para validar entrada do formulário de cliente
const validarCliente = (nome, cpf) => {
    if (!nome.trim()) {
        return 'Por favor, insira um nome válido.';
    }
    if (!/^\d{11}$/.test(cpf)) {
        return 'Por favor, insira um CPF válido com 11 dígitos numéricos.';
    }
    return null;
};

// Função para carregar clientes
const carregarClientes = async () => {
    try {
        clienteSelect.innerHTML = '<option value="" disabled selected>Carregando clientes...</option>';
        clienteSelect.disabled = true;

        const response = await fetch('http://localhost:5151/api/Banco/clientes', {
            headers: { 'Accept': 'application/json' }
        });

        if (!response.ok) {
            throw new Error('Erro ao carregar clientes');
        }

        const clientes = await response.json();

        if (clientes.length === 0) {
            clienteSelect.innerHTML = '<option value="" disabled selected>Nenhum cliente encontrado</option>';
            mostrarMensagem('Nenhum cliente disponível no momento.', true);
            return;
        }

        clientes.sort((a, b) => a.nome.localeCompare(b.nome));

        clienteSelect.innerHTML = '<option value="" disabled selected>Selecione um cliente</option>';
        clientes.forEach(cliente => {
            const option = document.createElement('option');
            option.value = cliente.id;
            const saldoText = cliente.saldo !== undefined ? ` - Saldo: R$${parseFloat(cliente.saldo).toFixed(2)}` : '';
            option.textContent = `${cliente.nome} (CPF: ${cliente.cpf})${saldoText}`;
            clienteSelect.appendChild(option);
        });

        clienteSelect.disabled = false;
    } catch (error) {
        clienteSelect.innerHTML = '<option value="" disabled selected>Erro ao carregar clientes</option>';
        mostrarMensagem('Erro ao carregar clientes: ' + error.message, true);
        clienteSelect.disabled = true;
    }
};

// Manipulador de submissão do formulário de transação
form.addEventListener('submit', async (event) => {
    event.preventDefault();

    resultadoDiv.innerText = '';
    resultadoDiv.classList.remove('erro', 'sucesso');

    const clienteId = parseInt(clienteSelect.value);
    const tipo = tipoSelect.value;
    const valor = parseFloat(valorInput.value);

    const erroValidacao = validarEntrada(clienteId, tipo, valor);
    if (erroValidacao) {
        mostrarMensagem(erroValidacao, true);
        return;
    }

    try {
        const response = await fetch(`http://localhost:5151/api/Banco/${tipo}/${clienteId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ valor })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `Erro HTTP: ${response.status}`);
        }

        const data = await response.json();
        mostrarMensagem(`Transação ${tipo} de R$${valor.toFixed(2)} realizada com sucesso!`);
        await carregarClientes();
    } catch (error) {
        mostrarMensagem(`Erro ao processar transação: ${error.message}`, true);
        console.error('Erro na requisição:', error);
    }
});

// Manipulador de submissão do formulário de cliente
clienteForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    resultadoDiv.innerText = '';
    resultadoDiv.classList.remove('erro', 'sucesso');

    const nome = document.getElementById('nome').value;
    const cpf = document.getElementById('cpf').value;

    const erroValidacao = validarCliente(nome, cpf);
    if (erroValidacao) {
        mostrarMensagem(erroValidacao, true);
        return;
    }

    try {
        const response = await fetch('http://localhost:5151/api/Banco/clientes', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ nome, cpf })
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || `Erro HTTP: ${response.status}`);
        }

        const data = await response.json();
        mostrarMensagem(`Cliente ${nome} criado com sucesso!`);
        clienteForm.reset();
        await carregarClientes();
    } catch (error) {
        mostrarMensagem(`Erro ao criar cliente: ${error.message}`, true);
        console.error('Erro na requisição:', error);
    }
});

// Carrega os clientes ao iniciar
document.addEventListener('DOMContentLoaded', carregarClientes);