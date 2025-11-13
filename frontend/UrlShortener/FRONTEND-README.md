# URL Shortener - Frontend

## 🚀 Sobre o Projeto

Interface moderna desenvolvida em Angular 20 para encurtamento de URLs, integrada com backend .NET. Utiliza Material Design 3, signals e as mais recentes práticas de desenvolvimento Angular.

## ✨ Funcionalidades Implementadas

### 🎯 Core Features
- **Encurtamento de URLs**: Interface intuitiva para criar URLs curtas
- **Validação em Tempo Real**: Validação de formato de URL com feedback visual
- **Cópia com Um Clique**: Botão para copiar a URL encurtada
- **Teste Direto**: Botão para testar o redirecionamento
- **Reset Rápido**: Função para limpar e criar nova URL

### 🎨 Design & UX
- **Material Design 3**: Interface moderna seguindo as diretrizes do Google
- **Tema Azure/Blue**: Paleta de cores profissional e acessível
- **Design Responsivo**: Otimizado para desktop, tablet e mobile
- **Animações Suaves**: Transições e feedback visual
- **Feedback Contextual**: Mensagens de sucesso, erro e carregamento

### 🔧 Tecnologia & Arquitetura
- **Angular 20**: Versão mais recente com signals
- **Standalone Components**: Arquitetura moderna sem módulos
- **Reactive Forms**: Validação robusta e reativa
- **Angular Material**: Componentes consistentes e acessíveis
- **TypeScript**: Tipagem forte e interfaces bem definidas
- **SCSS**: Estilos organizados e responsivos

## 📁 Estrutura Criada

```
src/
├── app/
│   ├── components/
│   │   ├── url-shortener.component.ts    # Componente principal
│   │   └── url-shortener.component.scss  # Estilos do componente
│   ├── services/
│   │   └── url-shortener.service.ts      # Serviço HTTP para API
│   ├── models/
│   │   └── url-response.model.ts         # Interfaces TypeScript
│   ├── app.config.ts                     # Configuração da app
│   ├── app.ts                           # Componente raiz
│   ├── app.html                         # Template principal
│   └── app.scss                         # Estilos globais
├── environments/
│   ├── environment.ts                   # Config desenvolvimento
│   └── environment.prod.ts              # Config produção
└── styles.scss                         # Reset CSS + Material
```

## 🔌 Integração com Backend

### Endpoints Utilizados
- `POST /` - Criar URL curta
  - **Request**: `{ longUrl: string }`
  - **Response**: `{ shortCode: string, longUrl?: string }`

- `GET /{shortCode}` - Redirecionamento (usado para teste)

### Configuração da API
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000'  // URL do backend
};
```

## 🎛️ Como Usar

### Para Desenvolvimento
1. **Iniciar**: `npm start`
2. **Acessar**: `http://localhost:4200`
3. **Testar**: Digite uma URL e clique em "Encurtar URL"

### Para Produção
1. **Build**: `npm run build`
2. **Configurar**: Atualizar `environment.prod.ts` com URL da API
3. **Deploy**: Usar arquivos da pasta `dist/`

## 🔍 Validações Implementadas

- ✅ **Campo Obrigatório**: URL não pode estar vazia
- ✅ **Formato URL**: Regex para validar URLs válidas
- ✅ **Protocolo**: Aceita apenas HTTP/HTTPS
- ✅ **Feedback Visual**: Mensagens de erro em tempo real

## 📱 Recursos de UX

### Estados da Interface
- **Idle**: Estado inicial para inserir URL
- **Loading**: Spinner durante o encurtamento
- **Success**: Exibição da URL encurtada com ações
- **Error**: Mensagem de erro com opção de retry

### Ações Disponíveis
- **Copiar**: Copia URL para clipboard com feedback
- **Testar**: Abre URL em nova aba para validar
- **Nova URL**: Limpa formulário para nova operação

## 🎨 Customizações

### Tema de Cores
O projeto usa o tema Azure/Blue do Material Design 3. Para alterar:

```scss
// Em styles.scss - alterar a configuração do tema
@include mat.theme((
  color: (
    primary: mat.$azure-palette,    // Altere aqui
    tertiary: mat.$blue-palette,    // E aqui
  ),
  // ...
));
```

### Responsividade
Breakpoints configurados:
- **Desktop**: > 768px
- **Mobile**: ≤ 768px

## 🔄 Próximos Passos (Sugestões)

1. **Histórico**: Salvar URLs encurtadas no localStorage
2. **Estatísticas**: Integrar com endpoint de analytics
3. **QR Code**: Gerar QR code da URL encurtada
4. **Múltiplas URLs**: Batch de encurtamento
5. **Dark Mode**: Toggle para tema escuro
6. **PWA**: Transformar em Progressive Web App

## 🧪 Para Testar

1. **URL Válida**: `https://www.google.com`
2. **URL Inválida**: `google.com` (sem protocolo)
3. **Campo Vazio**: Validação de campo obrigatório
4. **Cópia**: Testar função de copiar
5. **Redirecionamento**: Testar botão "Testar"

---

**Desenvolvido com Angular 20, Material Design 3 e boas práticas de desenvolvimento frontend.**