# Colaí - Clipboard Manager

O **Colaí - Clipboard Manager** é um gerenciador de área de transferência para Windows, desenvolvido para oferecer uma experiência simples, rápida e próxima ao histórico nativo do sistema, mas com persistência local e maior controle sobre os itens copiados.

O aplicativo permanece em execução na bandeja do sistema, monitora textos copiados e permite recuperar rapidamente qualquer item do histórico.

## Funcionalidades

- Captura automática de textos copiados para a área de transferência.
- Histórico persistente em banco de dados local SQLite.
- Itens mais recentes exibidos no topo da lista.
- Fixação e desafixação de itens importantes.
- Limpeza do histórico sem remover itens fixados.
- Exclusão individual de itens.
- Abertura rápida por atalho global `Win + Alt + V`.
- Colagem automática na aplicação anteriormente ativa com duplo clique.
- Execução em segundo plano com ícone na bandeja do sistema.
- Encerramento completo pelo menu do ícone na bandeja.
- Armazenamento totalmente local, sem necessidade de conta ou serviço externo.

## Tecnologias

O Colaí é desenvolvido com as seguintes tecnologias:

- **C#**
- **.NET 8**
- **WPF - Windows Presentation Foundation**
- **SQLite**
- **Microsoft.Data.Sqlite**
- **Windows API / Win32** para monitoramento da área de transferência e registro do atalho global
- **Windows Forms NotifyIcon** para integração com a bandeja do sistema

## Requisitos

- Windows 11
- .NET 8 SDK para compilar o projeto localmente

## Privacidade e armazenamento

O Colaí armazena localmente os textos copiados para a área de transferência. Os dados são mantidos em um banco SQLite no computador em que o aplicativo está sendo executado.

O aplicativo não precisa enviar o histórico a servidores externos para realizar suas funções atuais.

> Atenção: a área de transferência pode conter senhas, códigos de autenticação, dados pessoais e outras informações sensíveis. O usuário é responsável por avaliar os dados copiados e por proteger o acesso ao computador e ao banco local.

## Licença

O código-fonte do **Colaí - Clipboard Manager** é disponibilizado sob a **GNU General Public License v3.0**, também identificada como **GPL-3.0**.

Em resumo, a licença permite:

- Uso pessoal e corporativo;
- Estudo e modificação do código-fonte;
- Criação de forks;
- Cópia e redistribuição;
- dDstribuição gratuita ou comercial.

Ao distribuir o Colaí ou uma versão modificada, é necessário cumprir as condições da GPLv3, incluindo preservar os avisos aplicáveis, identificar alterações e disponibilizar o código-fonte correspondente sob a mesma licença.

Consulte o arquivo [`LICENSE`](LICENSE) para conhecer os termos completos. Em caso de divergência, prevalece o texto integral da licença.

## Nome e identidade visual

A GPLv3 aplica-se ao código-fonte do projeto. O uso do nome **Colaí - Clipboard Manager**, do logotipo e da identidade visual em versões derivadas poderá ser tratado separadamente por uma política de marca.

Para evitar confusão com a versão oficial, forks distribuídos publicamente devem utilizar identificação própria e informar sua origem no projeto Colaí - Clipboard Manager.

## Autor e contato

**Pedro Augusto Carlo**

- E-mail: `pedroaugustocarlo@gmail.com`
- Projeto: `https://github.com/pedroaugustocarlo/colai-clipboard-manager`

## Aviso de garantia

O software é fornecido sem garantias, conforme estabelecido pela GPLv3. O autor não garante que o aplicativo será adequado a todas as finalidades, ambientes ou configurações e não assume obrigação de suporte contínuo.
