# 📄 6A Workflow Standard Specification v1.0

## 1. Scope
This standard defines the **6A Workflow** methodology and execution rules. It applies to research, software development, engineering, and cross-team collaboration projects that require **goal alignment, task decomposition, automation, and closed-loop improvement**.  
This standard serves as a unified working method for team members during task management, development, and delivery.

---

## 2. Terms & Definitions
- **6A Workflow**: A task execution framework composed of six phases: Align, Architect, Atomize, Approve, Automate, and Assess.  
- **Deliverable**: The fixed artifact or result required at the end of each phase.  
- **Acceptance Criteria**: Measurable conditions that determine whether a task is considered complete.  

---

## 3. Process Definition

### 3.1 Align (Goal Alignment)
**Purpose**: Define background, objectives, scope, and expected outcomes; ensure stakeholder consensus.  
**Input**: Task requirements, background information.  
**Deliverable**: `ALIGNMENT.md` (objectives, scope, expected outcomes).  

---

### 3.2 Architect (Solution Design)
**Purpose**: Design the solution approach and execution path.  
**Input**: Requirements specification, task objectives.  
**Deliverable**: `DESIGN.md` (input/output definitions, analysis dimensions, methodology, workflow diagrams).  

---

### 3.3 Atomize (Task Decomposition)
**Purpose**: Break down the overall task into executable subtasks.  
**Input**: Solution design.  
**Deliverable**: `TASK.md` (subtask list, dependencies, time/effort estimation).  

---

### 3.4 Approve (Consensus & Validation)
**Purpose**: Confirm task breakdown and execution plan; define acceptance criteria.  
**Input**: Task decomposition document.  
**Deliverable**: `ACCEPTANCE.md` (acceptance criteria, review process, responsible parties).  

---

### 3.5 Automate (Execution Automation)
**Purpose**: Tool or automate repeatable workflows to minimize human error.  
**Input**: Task plan and standards.  
**Deliverable**: Automation scripts, CI/CD configurations, tool documentation.  

---

### 3.6 Assess (Evaluation & Improvement)
**Purpose**: Verify execution results against acceptance criteria and drive continuous improvement.  
**Input**: Deliverables, acceptance criteria.  
**Deliverable**: `FINAL.md` (final deliverable checklist), `TODO.md` (follow-up improvements), evaluation report.  

---

## 4. Documentation Framework
Each project must create the following files under `docs/6A/`:  

- `ALIGNMENT.md` – Goals & Scope  
- `DESIGN.md` – Solution Design  
- `TASK.md` – Task Decomposition  
- `ACCEPTANCE.md` – Acceptance Criteria  
- `FINAL.md` – Final Deliverables  
- `TODO.md` – Pending Improvements  

---

## 5. Execution Rules
1. All tasks must follow the 6A Workflow; no phase may be skipped.  
2. Each phase’s **deliverables must be completed** and committed to version control.  
3. **Acceptance criteria** must be defined in the Approve phase and used in Assess.  
4. All automation artifacts must be stored in the repository and documented.  
5. Each phase must undergo **review/approval** before proceeding to the next.  

---

## 6. Relation to Existing Frameworks (Mapping)

| 6A Workflow | PDCA | PMBOK | Scrum |  
|-------------|-------|--------|--------|  
| Align       | Plan  | Initiation/Scope Mgmt | Sprint Planning |  
| Architect   | Plan  | Planning             | Sprint Planning |  
| Atomize     | Do    | Execution (Work Breakdown) | Backlog Refinement |  
| Approve     | Check | Monitoring & Control | Sprint Review |  
| Automate    | Do    | Execution (Tools)    | CI/CD, DevOps |  
| Assess      | Act   | Closing/Improvement  | Retrospective |  

---

## 7. Acceptance Criteria
- Deliverables completed and committed for each phase.  
- Every task has clear goals, breakdown, and acceptance criteria.  
- Automation coverage (ratio of repeatable tasks automated) ≥ 50%.  
- High-priority issue resolution (P0/P1) ≥ 80%.  
- Review pass rate ≥ 90%.  

---

## 8. Versioning
- **v1.0**: Initial definition, covering general task management.  
- **v2.0 (planned)**: Introduce role responsibility assignment (RACI) and KPI indicators.  
- **v3.0 (planned)**: Support cross-organization collaboration and external audits.  

---

## 9. Visual Workflow Diagram
```mermaid
flowchart TD
    A[Align: Goals & Scope] --> B[Architect: Solution Design]
    B --> C[Atomize: Task Decomposition]
    C --> D[Approve: Consensus & Acceptance Criteria]
    D --> E[Automate: Tooling & Execution]
    E --> F[Assess: Evaluation & Improvement]
    F --> A[Continuous Loop]