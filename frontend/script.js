// Seleciona elementos do DOM
const form = document.getElementById('transacaoForm');
const resultadoDiv = document.getElementById('resultado');
const tipoSelect = document.getElementById('tipo');
const valorInput = document.getElementById('valor');

// Função para exibir mensagens ao usuário
const mostrarMensagem = (mensagem, isErro = false) => {
    resultadoDiv.innerText = mensagem;
    resultadoDiv.classList.toggle('erro', isErro);
    resultadoDiv.classList.toggle('sucesso', !isErro);
};

// Função para validar entrada
const validarEntrada = (tipo, valor) => {
    if (!tipo) {
        return 'Por favor, selecione um tipo de transação.';
    }
    if (isNaN(valor) || valor <= 0) {
        return 'Por favor, insira um valor numérico maior que zero.';
    }
    return null;
};

// Manipulador de submissão do formulário
form.addEventListener('submit', async (event) => {
    event.preventDefault();

    // Limpa mensagens anteriores
    resultadoDiv.innerText = '';
    resultadoDiv.classList.remove('erro', 'sucesso');

    // Obtém valores do formulário
    const tipo = tipoSelect.value;
    const valor = parseFloat(valorInput.value);

    // Valida entrada
    const erroValidacao = validarEntrada(tipo, valor);
    if (erroValidacao) {
        mostrarMensagem(erroValidacao, true);
        return;
    }

    try {
        // Configura a requisição
        const response = await fetch(`http://localhost:5151/api/Banco/${tipo}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify({ valor })
        });

        // Verifica se a resposta foi bem-sucedida
        if (!response.ok) {
            throw new Error(`Erro HTTP: ${response.status}`);
        }

        const data = await response.json();

        // Exibe resultado formatado
        mostrarMensagem(`Transação ${tipo} de R$${valor.toFixed(2)} realizada com sucesso!`);
    } catch (error) {
        // Trata erros de rede ou do servidor
        mostrarMensagem(`Erro ao processar transação: ${error.message}`, true);
        console.error('Erro na requisição:', error);
    }
});