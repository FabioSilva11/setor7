# Setor 07 — adaptação visual dos HTMLs

Referências: `../Interface HTML/index.html`, `styles.css` e `loading.html` atuais. O usuário confirmou o projeto Third Person Gameplay.

Reproduzir lobby, formação, ajustes, equipamento, loja, eventos e cofre como objetos Unity nativos editáveis; dados simulados em um ScriptableObject. Não conectar partida, rede, compras, recompensas ou configurações reais. A escolha da tela para revisão fica no Inspector.

Usar uGUI para atender à biblioteca Unity UI Extensions pedida; reaproveitar os ícones Lucide locais licenciados. A UI Toolkit existente permanece com as cenas anteriores. Uma cena de apresentação própria será a primeira cena de build, sem iniciar os sistemas de gameplay.

Alternativas consideradas: alterar a UI Toolkit atual exigiria a variante UIToolkit Extensions, diferente da biblioteca solicitada; usar um navegador embutido dificultaria a edição no Unity. Escolha técnica: uGUI com prefabs, texto, imagens, botões e campos serializados nativos.

Loading: reproduzir fundo escuro, texto e barra fina inferior. Carregar o prefab da interface por Resources.LoadAsync, aguardar sua criação e atualização do Canvas, e somente então revelar o lobby. Tempo mínimo configurável, sem porcentagem aleatória. O caminho da imagem de fundo ficará preparado como referência opcional de Sprite.

Validar compilação, testes EditMode, início real em PlayMode, todas as telas e capturas Unity em 1280×720 e resolução mais larga. Atualização explícita do usuário: personagem desarmado no centro e fundo dentro de um galpão; o fundo escuro do HTML servia somente para concentrar nos menus. Construir um cenário visual industrial e reutilizar o personagem local em idle, sem componentes de gameplay.
