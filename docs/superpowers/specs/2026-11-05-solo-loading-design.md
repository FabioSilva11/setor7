# Solo e entrada na partida

Pedido: preservar a versão atual no GitHub como setor7 antes das alterações; ativar a seleção Solo e o botão JOGAR. Duo e Squad continuam visíveis e sem ação.

Reutilizar lobby uGUI e carregamento existentes. O clique abre o loading opaco antes do trabalho pesado. Carregar Shooter de forma assíncrona, iniciar o host existente e aguardar jogador, câmera e arma prontos antes de revelar a partida. Não criar matchmaking nem regras de equipe.

Escolha técnica: carregar a cena em modo Single mantendo apenas o Canvas do loading entre cenas, em ScreenSpaceOverlay. Carregamento aditivo manteria dois cenários na memória; trocar de cena sem Canvas persistente causaria flashes e ocultaria o progresso.

Solo permanece selecionado. O seletor abre e fecha por clique; Duo/Squad não mudam a seleção. Cliques repetidos em JOGAR não iniciam novas operações. Falhas deixam uma mensagem e permitem voltar ao lobby.

Preservar os demais menus como apresentação visual. Garantir EventSystem com Input System no lobby. Interface será a cena inicial de build e Shooter continuará incluída. Atualizar o helper Android para incluir ambas.

Validar botões reais e Play Mode, incluindo loading visível até jogador e arma existirem. Revisar o diff e verificar Console antes de entregar.
