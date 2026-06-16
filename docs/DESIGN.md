# Design Doc — DevClinic: Início de Atendimento a partir de Agendamento

## 1. Contexto

O DevClinic é um sistema para controle de uma clínica, contemplando agendamentos, atendimentos clínicos, prontuário, notificações e financeiro.

Atualmente, o sistema possui os bounded contexts candidatos Scheduling e Clinical. Scheduling lida com agendamentos, enquanto Clinical lida com atendimentos e prontuário.

O aggregate Appointment pertence ao contexto Scheduling e é persistido de forma state-based com EF Core. O aggregate Attendance pertence ao contexto Clinical e é persistido usando Event Sourcing.

Um Appointment representa uma reserva de horário. Um Attendance representa um atendimento clínico efetivamente iniciado.

## 2. Problema

Precisamos definir como um atendimento clínico (Attendance) deve ser iniciado a partir de um agendamento (Appointment) válido.

O Appointment pertence ao contexto de Scheduling e representa uma reserva de horário entre paciente e médico. Já o Attendance pertence ao contexto de Clinical e representa o atendimento clínico efetivamente iniciado, com regras próprias para registro de queixa, diagnóstico, prescrição e fechamento.

Como essa transição envolve dois bounded contexts, é necessário definir onde as validações devem acontecer, quais regras pertencem ao Appointment, quais pertencem ao Attendance e como garantir que um mesmo agendamento não origine mais de um atendimento.

Se essa integração for feita sem uma decisão clara, o sistema pode permitir atendimentos duplicados, iniciar atendimentos a partir de agendamentos inválidos, misturar responsabilidades entre Scheduling e Clinical ou criar acoplamento excessivo entre os módulos.

## 3. Objetivos

* Garantir que apenas `Appointments` válidos possam iniciar um `Attendance`.
* Garantir que o `Appointment` esteja em um estado permitido para início do atendimento.
* Garantir que apenas o médico responsável pelo `Appointment` possa iniciar o `Attendance`.
* Garantir que um mesmo `Appointment` não origine mais de um `Attendance`.
* Manter rastreabilidade entre `Attendance` e `Appointment` por meio do `AppointmentId`.
* Preservar a separação entre os contextos `Scheduling` e `Clinical`, evitando acoplamento excessivo entre seus modelos internos.
* Tratar cenários de concorrência, retornando conflito quando houver tentativa de criar mais de um `Attendance` para o mesmo `Appointment`.
* Manter a operação consistente, evitando que o `Appointment` seja marcado como iniciado sem que o `Attendance` correspondente seja criado.


## 4. Fora de escopo

* Não vamos implementar o contexto/módulo de Billing neste momento.
* Não vamos implementar o contexto/módulo de Notifications neste momento.
* Não vamos refatorar o `MedicalRecord` para projeção/read model agora, mesmo existindo a hipótese de que isso faça mais sentido no futuro.
* Não vamos adotar uma arquitetura de microsserviços. A solução continuará considerando o DevClinic como um monólito modular.
* Não vamos introduzir mensageria assíncrona para iniciar o atendimento a partir do agendamento neste momento.
* Não vamos redesenhar toda a arquitetura do DevClinic; o foco deste design doc é o fluxo de início de `Attendance` a partir de um `Appointment`.


## 5. Modelo atual

* `Scheduling` é um bounded context candidato responsável pelo gerenciamento de agendamentos. Ele lida com regras relacionadas à criação, confirmação, cancelamento e início do fluxo de atendimento a partir de um agendamento.
* `Appointment` é o aggregate principal dentro do contexto `Scheduling`. Ele representa uma reserva de horário entre paciente e médico e é persistido de forma state-based usando EF Core.
* `Clinical` é um bounded context candidato responsável pelos atendimentos clínicos e pelo prontuário. Ele lida com o registro da queixa, diagnóstico, prescrição, fechamento do atendimento e informações clínicas associadas ao paciente.
* `Attendance` é um aggregate dentro do contexto `Clinical`, persistido usando Event Sourcing. Ele representa um atendimento clínico efetivamente iniciado e protege regras relacionadas ao seu ciclo de vida, como início, registro de informações clínicas, emissão de prescrição e fechamento.
* `MedicalRecord` é um aggregate dentro do contexto `Clinical`, também persistido usando Event Sourcing. Ele representa o prontuário clínico do paciente. Existe uma lacuna conhecida sobre se esse modelo deveria continuar como aggregate independente ou se faria mais sentido, no futuro, ser tratado como uma projeção/read model derivada dos eventos clínicos.
* `Billing` e `Notifications` ainda não foram implementados e estão fora do escopo deste design doc. A integração futura com Billing deverá ocorrer a partir de um evento de integração derivado do fechamento de um atendimento faturável.


## 6. Fluxo proposto

Descreva o passo a passo do caso de uso.

## 7. Regras e invariantes

Liste quais regras ficam em Appointment, Attendance, Application e Infrastructure.

## 8. Decisões

Registre as decisões tomadas.

## 9. Trade-offs

Explique vantagens e custos da abordagem.

## 10. Riscos e lacunas

Liste pontos que precisam ser revisitados no futuro.

## 11. Próximos passos

Liste ações concretas.