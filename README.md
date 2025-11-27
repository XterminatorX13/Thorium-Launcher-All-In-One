# Thorium Launcher — All-in-One

Uma launcher leve, portátil e repleta de recursos para o **Thorium Browser** (e outros navegadores baseados no Chromium), escrita em **C# (WinForms)**. Projetada para gerenciar múltiplos perfis, manipular flags avançadas e fornecer modos de login seguros sem precisar de instalações complexas.

## Features

### Profile Management
- **Múltiplos Perfis**: Crie, clone e gerencie perfis isolados sem limite de quantidade.
- **Gerenciador de Perfis**: Visualize estatísticas, **Renomeie** e exclua perfis facilmente.
- **Ações Rápidas**: Clone perfis existentes ou exclua-os com um clique.
- **Dados Portáteis**: Todos os perfis são armazenados no subdiretório `Profiles/`.

### 🛡️ Modos de Privacidade e Segurança
- **Modo Seguro (Login)**: Lança uma sessão temporária ou persistente mínima para o login do Google.
- **Modo Reforçado**: Detecta automaticamente e aplica suas flags avançadas.
- **Modo Efêmero**: Lança uma sessão temporária que se apaga ao ser fechada.

### ⚙️ Configuração Avançada
- **Editor de Flags**: Edite as flags do Chromium diretamente na launcher.
- **Atalhos de Desktop**: Exporte sua configuração para um arquivo `.bat` e crie automaticamente um atalho de Desktop com o ícone correto.
- **Auto-Centralização**: Calcula e centraliza automaticamente a janela do navegador.

## Como Compilar (Sem SDK Necessário)

Você pode compilar este projeto utilizando o compilador **C# nativo** incluído no Windows. Não é necessário instalar o Visual Studio ou o SDK do .NET.

1. Abra o **Prompt de Comando (cmd)** na pasta do projeto.
2. Execute o comando de compilação:

   ```cmd
   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:ThoriumLauncher.exe /win32icon:"Umbra Puprpurea.ico" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:Microsoft.VisualBasic.dll /reference:System.Core.dll Program.cs
``


3. Pronto! O `ThoriumLauncher.exe` será criado.

## Uso

1. **Selecionar Executável**: Aponte a launcher para o seu `thorium.exe`.
2. **Escolher Perfil**: Selecione "Padrão" ou crie um novo.
3. **Personalizar Flags**: Adicione suas flags na caixa de texto (ou deixe as padrão).
4. **Lançar**: Clique em "LAUNCH" (ou pressione **Alt+L**).

## Estrutura de Pastas & Dados de Sessão

A launcher organiza todos os dados de perfil em uma estrutura limpa e portátil:

```
thorium_all_in_one/
├── ThoriumLauncher.exe          # A própria launcher
├── Umbra Puprpurea.ico          # Ícone customizado (opcional)
├── launcher.ini                 # Configurações da launcher (caminho do exe, último perfil)
└── Profiles/                    # Todos os dados de perfil (PORTÁVEL!)
    ├── thorium-profile/         # Pasta do perfil padrão
    │   ├── flags.txt            # Flags do perfil padrão
    │   ├── Cookies              # Cookies da sessão
    │   ├── Login Data           # Senhas salvas
    │   ├── History              # Histórico de navegação
    │   ├── Local Storage/       # Dados de sites
    │   └── ... (todos os dados do usuário do Chromium)
    │
    └── thorium-profile-NAME/    # Pasta do perfil personalizado
        ├── flags.txt            # Flags do perfil personalizado
        └── ... (dados de sessão isolados)
```

**Onde estão meus dados de sessão?**

* Todos os dados do navegador (cookies, histórico, senhas, cache) são armazenados em `Profiles/thorium-profile-[NAME]/`
* Cada perfil é completamente isolado, com seus próprios dados de sessão.
* O arquivo `flags.txt` dentro de cada pasta de perfil contém as flags de linha de comando.
* Você pode fazer backup de perfis inteiros copiando suas pastas.

**Portabilidade:**

* Toda a pasta `Profiles/` pode ser movida para outro computador.
* Basta copiar a pasta e atualizar o caminho do executável na launcher.

## Exportando Perfis

Ao clicar em **"Exportar"**, a launcher salva um arquivo `.bat` e oferece a opção de criar atalhos:

### Opções de Exportação:

* **Sim** = Cria um atalho **direto .lnk** (recomendado!)

  * Aponta diretamente para `thorium.exe` com TODAS as flags no campo de Argumentos
  * Contorna o limite de 260 caracteres do Windows para a GUI
  * Suporta até **4096 caracteres** de flags programaticamente
  * **Pode ser fixado na barra de tarefas** sem perder suas configurações!
  * Arquivo criado: `Thorium - [ProfileName].lnk` na área de trabalho

* **Não** = Pula a criação de atalhos (salva apenas o arquivo .bat)

* **Cancelar** = Cria AMBOS os atalhos:

  * Atalho direto .lnk (para fixação na barra de tarefas)
  * Atalho .bat (para compatibilidade com versões anteriores)

### Por que o atalho direto .lnk é melhor:

✅ **Fixação na barra de tarefas funciona!** O Windows não remove suas flags

✅ Sem janela do CMD (executa silenciosamente)

✅ Suporta listas de flags muito longas (milhares de caracteres)

✅ Suporte a ícones personalizados

✅ Funciona exatamente como um atalho nativo do Windows

**Nota:** O arquivo `.bat` ainda é útil para automação ou se você preferir arquivos em batch, mas o atalho `.lnk` é a melhor opção para o uso diário e fixação na barra de tarefas.

## Integração Nativa de Perfil Thorium (NOVO!)

Agora a launcher **detecta automaticamente** perfis existentes do Thorium na pasta de instalação do navegador!

### Como funciona:

1. **Detecção Automática**: Ao selecionar um executável do Thorium, a launcher escaneia a pasta `User Data`
2. **Perfis Nativos Aparecem**: Perfis do Thorium (como "Profile 1", "Profile 2", "Default") aparecem no dropdown com o prefixo **[Native]**
3. **Adicionar Flags**: Você pode adicionar flags personalizadas a qualquer perfil nativo - elas serão salvas em `User Data/[ProfileName]/flags.txt`
4. **Integração Transparente**: Lançar perfis nativos com suas flags personalizadas ou usá-los como estão.

### Exemplo:

```
Dropdown mostra:
- Default                    ← Perfil gerenciado pela launcher
- MyCustomProfile            ← Perfil gerenciado pela launcher
- [Native] Default           ← Perfil nativo do Thorium
- [Native] Profile 1         ← Perfil nativo do Thorium
- [Native] Profile 2         ← Perfil nativo do Thorium
```

### Benefícios:

✅ **Sem necessidade de migração** - Use seus perfis existentes do Thorium imediatamente
✅ **Adicione flags aos perfis existentes** - Melhore perfis nativos com flags personalizadas
✅ **Gerenciamento unificado** - Gerencie tanto perfis da launcher quanto nativos em um só lugar
✅ **Preserve os dados do navegador** - Mantenha todos os seus cookies, histórico e configurações

**Nota:** Perfis nativos estão localizados em `[ThoriumDir]/User Data/[ProfileName]/` e são marcados com o prefixo `[Native]` para distingui-los dos perfis gerenciados pela launcher em `Profiles/thorium-profile-[NAME]/`.

## FAQ

### Q: As flags de linha de comando aparecem como "Ativadas" em `chrome://flags`?

**A:** Não. As flags passadas via linha de comando (como em arquivos .bat ou atalhos) NÃO aparecem como "Ativadas" na interface `chrome://flags`. Essa interface só controla preferências salvas no arquivo `Local State` do navegador.

Para verificar se suas flags estão ativas, acesse **chrome://version** e confira a seção "Command Line". Se suas flags aparecerem lá, elas estão funcionando.

### Q: Por que meu navegador parece "padrão" mesmo com as flags ativadas?

**A:** Se o navegador parecer padrão, é porque as flags de personalização visual podem não estar presentes ou a versão do Thorium alterou seu comportamento. Sempre verifique as flags ativas em **chrome://version**.

### Q: Por que o arquivo .bat exportado não fecha automaticamente?

**A:** Isso foi corrigido na versão mais recente. O arquivo .bat gerado agora inclui um comando `exit` para fechar a janela CMD automaticamente após iniciar o navegador.

### Q: Como fixar a launcher/atalho na barra de tarefas do Windows?

**A:** Quando você exporta um arquivo .bat, a launcher agora oferece a opção de criar um
### Q: Como fixar a launcher/atalho na barra de tarefas do Windows?
**A:** Quando você exporta um arquivo .bat, a launcher agora oferece a opção de criar um atalho **direto .lnk** que resolve o problema de fixação!

**O Problema:** Quando você fixa um atalho .bat na barra de tarefas, o Windows cria um NOVO atalho que aponta diretamente para `thorium.exe` SEM suas flags personalizadas. Isso significa que suas preferências não serão carregadas.

**A Solução (NOVO!):**
Ao exportar, escolha **"Sim"** ou **"Cancelar"** para criar um atalho direto `.lnk` que:
- Aponta diretamente para `thorium.exe` com TODAS as suas flags como argumentos
- Contorna o limite de 260 caracteres do Windows para a GUI (suporta até 4096 caracteres programaticamente)
- **PODERÁ ser fixado na barra de tarefas** e manterá TODAS as suas preferências!
- Nome do arquivo: `Thorium - [ProfileName].lnk` na sua Área de Trabalho

**Opções ao exportar:**
- **Sim** = Cria o atalho direto .lnk (recomendado - pode ser fixado!)
- **Não** = Pula a criação de atalhos (salva apenas o arquivo .bat)
- **Cancelar** = Cria AMBOS os atalhos: .lnk e .bat

**Soluções alternativas:**
1. **Fixar a própria Launcher**: Clique com o botão direito em `ThoriumLauncher.exe` → "Fixar na barra de tarefas" (e use-a para iniciar seus perfis)
2. **Método manual de fixação**: Copie o atalho .lnk para `%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar`

**Nota:** O atalho direto .lnk é a melhor opção para fixação na barra de tarefas mantendo todas as suas flags personalizadas!

---

## Requisitos
- **Windows 10/11**
- **.NET Framework 4.7.2** (pré-instalado na maioria dos sistemas Windows)

---

## FAQ

### Q: As flags de linha de comando aparecem como "Ativadas" em `chrome://flags`?
**A:** Não. As flags passadas via linha de comando (como em arquivos .bat ou atalhos) **não aparecem** como "Ativadas" na interface `chrome://flags`. Essa interface só controla preferências salvas no arquivo `Local State` do navegador.

Para verificar se suas flags estão ativas, acesse **chrome://version** e confira a seção "Command Line". Se suas flags aparecerem lá, elas estão funcionando.

### Q: Por que meu navegador parece "padrão" mesmo com as flags ativadas?
**A:** Se o navegador parecer padrão, é porque as flags de personalização visual podem não estar presentes ou a versão do Thorium alterou seu comportamento. Sempre verifique as flags ativas em **chrome://version**.

### Q: Por que o arquivo .bat exportado não fecha automaticamente?
**A:** Isso foi corrigido na versão mais recente. O arquivo .bat gerado agora inclui um comando `exit` para fechar a janela CMD automaticamente após iniciar o navegador.

### Q: Por que minha janela do navegador não está centralizada?
**A:** O perfil padrão "Hardened" usa `--start-maximized` em vez de `--window-position`. Se você quiser uma janela centralizada:
- Adicione manualmente `--window-position=X,Y` às suas flags, ou
- Use o botão "Testar Execução" → "Login Efêmero: Não", que calcula automaticamente a posição central.

### Q: Por que não consigo fazer login no Google com o perfil padrão?
**A:** O perfil padrão "Hardened" contém flags focadas em privacidade, como `--disable-background-networking` e opções anti-fingerprint, que intencionalmente bloqueiam o login do Google.

**Solução:** Crie um novo perfil "Padrão" sem essas flags de segurança, caso precise de acesso à conta Google.

### Q: As flags são compatíveis entre diferentes navegadores Chromium (Thorium, Ungoogled, etc.)?
**A:** As flags dependem da versão do Chromium e da versão específica do fork. Cada navegador pode suportar flags diferentes, então sempre verifique em `chrome://flags` se a flag é suportada ou descontinuada. As flags descontinuadas simplesmente serão ignoradas pelo navegador.

### Q: Se eu adicionar flags a um perfil nativo, elas serão salvas permanentemente?
**A:** Sim! Quando você seleciona um perfil nativo (marcado com o prefixo `[Native]`) e salva flags, elas são gravadas em um arquivo `flags.txt` dentro da pasta desse perfil. Sempre que você iniciar esse perfil pela launcher, as flags serão aplicadas automaticamente.

**Exemplo:**
1. Selecione `[Native] Profile 1`
2. Edite as flags e clique em "Salvar" → Cria o arquivo `User Data\Profile 1\flags.txt`
3. Clique em "Lançar" → O Thorium abre com os dados do Perfil 1 + suas flags personalizadas
4. Na próxima vez que lançar → As mesmas flags serão aplicadas automaticamente!

**Importante:** Seus dados de navegação (cookies, senhas, histórico) nunca são alterados. A launcher apenas adiciona as flags de desempenho/privacidade em cima do seu perfil existente.

---

## License

Este projeto é licenciado sob a [MIT License](LICENSE).
