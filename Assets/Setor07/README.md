# Setor / 07 — interface visual

Abra `Scenes/Interface.unity` e pressione Play. A tela de carregamento espera `Resources.LoadAsync` do prefab e a atualização do Canvas antes de revelar o lobby. O componente **InterfaceBootstrap**, no Canvas, permite editar o tempo mínimo e definir uma imagem de fundo opcional.

## Editar as telas

Abra `Resources/Setor07/Interface.prefab` em Prefab Mode e selecione **InterfacePresentation / Preview Screen**: Lobby, Formation, Settings, Inventory, Store, Events ou Vault. O prefab é visível para edição; o carregamento oculta a instância imediatamente ao criá-la e só a revela depois de atualizar seu layout.

No Play Mode, o objeto `Interface(Clone)` tem a mesma seleção **Preview Screen** para revisar os painéis. Os botões não têm ações: nenhuma partida, compra, resgate, ajuste real ou conexão de rede é iniciado. A seleção de telas é uma ferramenta de revisão no Inspector.

`MockData.asset` contém nome, saldos, modo, formação, volumes e produtos simulados. Campos principais são aplicados ao iniciar a apresentação. Para regenerar textos do catálogo, use **Setor 07 / Generate HTML interface assets**. Este comando recria o prefab e a cena; faça cópia das personalizações manuais antes de executá-lo. Para ajustes de layout, edite diretamente os RectTransforms, Texts, Images e Buttons do prefab.

O galpão está na cena sob **Warehouse Stage - visual only**, separado do Canvas. Piso, vigas, luzes e caixas são objetos editáveis. **Unarmed Operator - idle visual** usa o prefab existente do projeto, sem armas ou comportamento de jogo. A câmera determina seu enquadramento. Os materiais estão em `Art/Materials`.

## Bibliotecas e origem

- Unity UI Extensions 3.0.0 está incorporada em `Packages/com.unity.uiextensions`. O componente Gradient2 é usado no carregamento e na proteção de contraste dos menus. Licença e correções de compatibilidade ficam dentro do pacote.
- Os ícones Lucide existentes em `Assets/UI/Lucide` são usados como sprites nativos, com os SVGs originais e licença preservados.
- Fonte: Liberation Sans já incluída no projeto, como alternativa livre às fontes de sistema usadas nos HTMLs.
- Referências: `../Interface HTML/index.html`, `styles.css`, `loading.html`; fundo e personagem conforme orientação posterior do usuário.

A nova cena é a primeira em Build Settings. As cenas anteriores permanecem na lista e podem ser abertas diretamente.

## Verificar e capturar

O menu de geração monta os assets. Para revisão automatizada, execute no Unity `Setor07.Editor.InterfaceReviewCapture.Run`: gera a cena, entra em Play Mode, testa o caminho real do carregamento e grava PNGs em `Logs/Setor07/Captures`. Essa rotina encerra o Editor ao terminar e destina-se à execução em lote. Os testes ficam em `Tests/Editor`.
