// Seleção de elementos do DOM
const elements = {
  transacaoForm: document.getElementById('transacaoForm'),
  clienteForm: document.getElementById('clienteForm'),
  resultadoDiv: document.getElementById('resultado'),
  tipoSelect: document.getElementById('tipo'),
  valorInput: document.getElementById('valor'),
  clienteSelect: document.getElementById('clienteId'),
  nomeInput: document.getElementById('nome'),
  cpfInput: document.getElementById('cpf'),
};

// Funções utilitárias
const mostrarMensagem = (mensagem, isErro = false) => {
  elements.resultadoDiv.innerText = mensagem;
  elements.resultadoDiv.classList.toggle('erro', isErro);
  elements.resultadoDiv.classList.toggle('sucesso', !isErro);
};

const validarEntradaTransacao = (clienteId, tipo, valor) => {
  if (!clienteId) return 'Por favor, selecione um cliente.';
  if (!tipo) return 'Por favor, selecione um tipo de transação.';
  if (isNaN(valor) || valor <= 0) return 'Por favor, insira um valor numérico maior que zero.';
  if (!/^\d+(\.\d{1,2})?$/.test(valor)) return 'O valor deve ter no máximo duas casas decimais.';
  return null;
};

const validarCliente = (nome, cpf) => {
  if (!nome.trim()) return 'Por favor, insira um nome válido.';
  if (!/^\d{11}$/.test(cpf)) return 'Por favor, insira um CPF válido com 11 dígitos numéricos.';
  return null;
};

// Funções de API
const api = {
  async fetchClientes() {
    const response = await fetch('http://localhost:5151/api/Banco/clientes', {
      headers: { 'Accept': 'application/json' },
    });
    if (!response.ok) throw new Error('Erro ao carregar clientes');
    return await response.json();
  },

  async criarCliente(nome, cpf) {
    const response = await fetch('http://localhost:5151/api/Banco/clientes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: JSON.stringify({ nome, cpf }),
    });
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.mensagem || `Erro HTTP: ${response.status}`);
    }
    return await response.json();
  },

  async realizarTransacao(clienteId, tipo, valor) {
    const response = await fetch(`http://localhost:5151/api/Banco/${tipo}/${clienteId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      },
      body: JSON.stringify({ valor }),
    });
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.mensagem || `Erro HTTP: ${response.status}`);
    }
    return await response.json();
  },
};

// Função para carregar e exibir clientes
const carregarClientes = async () => {
  try {
    elements.clienteSelect.innerHTML = '<option value="" disabled selected>Carregando clientes...</option>';
    elements.clienteSelect.disabled = true;

    const clientes = await api.fetchClientes();

    if (clientes.length === 0) {
      elements.clienteSelect.innerHTML = '<option value="" disabled selected>Nenhum cliente encontrado</option>';
      mostrarMensagem('Nenhum cliente disponível no momento.', true);
      return;
    }

    clientes.sort((a, b) => a.nome.localeCompare(b.nome));

    elements.clienteSelect.innerHTML = '<option value="" disabled selected>Selecione um cliente</option>';
    clientes.forEach(cliente => {
      const option = document.createElement('option');
      option.value = cliente.id;
      option.dataset.saldo = cliente.saldo; // Armazena o saldo no dataset
      const saldoText = cliente.saldo !== undefined ? ` - Saldo: R$${parseFloat(cliente.saldo).toFixed(2)}` : '';
      option.textContent = `${cliente.nome} (CPF: ${cliente.cpf})${saldoText}`;
      elements.clienteSelect.appendChild(option);
    });

    elements.clienteSelect.disabled = false;
  } catch (error) {
    elements.clienteSelect.innerHTML = '<option value="" disabled selected>Erro ao carregar clientes</option>';
    mostrarMensagem(`Erro ao carregar clientes: ${error.message}`, true);
    elements.clienteSelect.disabled = true;
  }
};

// Manipuladores de eventos
const handleTransacaoSubmit = async (event) => {
  event.preventDefault();
  elements.resultadoDiv.innerText = '';
  elements.resultadoDiv.classList.remove('erro', 'sucesso');

  const clienteId = parseInt(elements.clienteSelect.value);
  const tipo = elements.tipoSelect.value;
  const valor = parseFloat(elements.valorInput.value);
  const selectedOption = elements.clienteSelect.options[elements.clienteSelect.selectedIndex];
  const saldo = parseFloat(selectedOption.dataset.saldo);

  const erroValidacao = validarEntradaTransacao(clienteId, tipo, valor);
  if (erroValidacao) {
    mostrarMensagem(erroValidacao, true);
    return;
  }

  if (tipo === 'saque' && saldo === 0) {
    mostrarMensagem('Não é possível realizar saque com saldo zero.', true);
    return;
  }

  try {
    await api.realizarTransacao(clienteId, tipo, valor);
    mostrarMensagem(`Transação ${tipo} de R$${valor.toFixed(2)} realizada com sucesso!`);
    await carregarClientes();
  } catch (error) {
    mostrarMensagem(`Erro ao processar transação: ${error.message}`, true);
    console.error('Erro na requisição:', error);
  }
};

const handleClienteSubmit = async (event) => {
  event.preventDefault();
  elements.resultadoDiv.innerText = '';
  elements.resultadoDiv.classList.remove('erro', 'sucesso');

  const nome = elements.nomeInput.value;
  const cpf = elements.cpfInput.value;

  const erroValidacao = validarCliente(nome, cpf);
  if (erroValidacao) {
    mostrarMensagem(erroValidacao, true);
    return;
  }

  try {
    await api.criarCliente(nome, cpf);
    mostrarMensagem(`Cliente ${nome} criado com sucesso!`);
    elements.clienteForm.reset();
    await carregarClientes();
  } catch (error) {
    mostrarMensagem(`Erro ao criar cliente: ${error.message}`, true);
    console.error('Erro na requisição:', error);
  }
};

// Função para alternar entre formulários
const alternarFormularios = () => {
  document.getElementById('showTransacao').addEventListener('click', () => {
    document.getElementById('transacaoSection').classList.add('active');
    document.getElementById('clienteSection').classList.remove('active');
    document.getElementById('showTransacao').classList.add('active');
    document.getElementById('showCliente').classList.remove('active');
  });

  document.getElementById('showCliente').addEventListener('click', () => {
    document.getElementById('clienteSection').classList.add('active');
    document.getElementById('transacaoSection').classList.remove('active');
    document.getElementById('showCliente').classList.add('active');
    document.getElementById('showTransacao').classList.remove('active');
  });
};

// Inicialização
const init = () => {
  elements.transacaoForm.addEventListener('submit', handleTransacaoSubmit);
  elements.clienteForm.addEventListener('submit', handleClienteSubmit);
  alternarFormularios();
  carregarClientes();
};

// Executa a inicialização quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', init);