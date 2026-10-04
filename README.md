# WINQUICK 1.0

Sistema desktop profissional de POS e gestão comercial, com facturação, stock, compras, clientes, fornecedores, caixa, pagamentos e módulos opcionais de restauração.

## Princípios

- Produto de produção, não mockup.
- MZN/MT e português como configuração principal.
- Operações financeiras e de stock transaccionais.
- Idempotência para operações críticas.
- Auditoria das operações sensíveis.
- Preparado para múltiplos terminais em LAN.
- Offline com fila e sincronização determinística.
- Sem GitHub Actions/workflows para a implementação do projecto.

## Arquitectura

- `src/WinQuick.Core`: domínio e regras de negócio.
- `src/WinQuick.Infrastructure`: EF Core, SQLite, persistência e migrations.
- `src/WinQuick.Server`: API central para LAN/multi-terminal.
- `src/WinQuick.Desktop`: aplicação WPF.
- `tests`: testes unitários e de integração.

## Facturação

A facturação é um módulo central do produto e não um recurso exclusivo de restauração. O domínio suporta séries, numeração, documentos, IVA, clientes, recibos, notas de crédito/débito, anulação, auditoria e ligação transaccional ao POS.

## Estado

O repositório está a ser construído incrementalmente. Uma funcionalidade só deve ser marcada como concluída depois de ter persistência, regras de negócio, tratamento de erros e testes correspondentes.
