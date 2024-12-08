using FamicomSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Config
{
    internal class AssembleTable
    {
        internal struct InstructionConfig
        {
            public Instruction Instruction;
            public AddressingMode AddressingMode;
            public byte ByteSize;
            public byte Cycle;
            public bool AddCycleWhenPageCrossed;

            public InstructionConfig(Instruction instruction, AddressingMode addressingMode)
            {
                Instruction = instruction;
                AddressingMode = addressingMode;
            }
        }

        internal Dictionary<AddressingMode, (int cycles, bool add)> AddressingModeCycleDic = new Dictionary<AddressingMode, (int, bool)>()
        {
            { AddressingMode.ZeroPage, (3, false)},
            //{ AddressingMode.Implicit, 2},
            //{ AddressingMode.Accumulator, 2},
            { AddressingMode.Immediate, (2, false)},
            { AddressingMode.ZeroPageX, (4, false)},
            { AddressingMode.ZeroPageY, (4, false)},
            { AddressingMode.Absolute, (4, false)},
            { AddressingMode.AbsoluteX, (4, true)},
            { AddressingMode.AbsoluteY, (4, true)},
            //{ AddressingMode.Indirect, 6},
            { AddressingMode.IndirectX, (6, false)},
            { AddressingMode.IndirectY, (5, true)},
            //{ AddressingMode.Relative, 2},
        };

        //internal List<InstructionConfig> InstructionCofigTable = new List<InstructionConfig>()
        //{
        //    new (Instruction.LDA, AddressingMode.Immediate, 2, 2),
        //    new (Instruction.LDA, AddressingMode.ZeroPage, 2, 3),
        //    new (Instruction.LDA, AddressingMode.ZeroPageX, 2, 4),
        //    new (Instruction.LDA, AddressingMode.Absolute, 3, 4),
        //    new (Instruction.LDA, AddressingMode.AbsoluteX, 3, 4, true),
        //    new (Instruction.LDA, AddressingMode.AbsoluteY, 3, 4, true),
        //    new (Instruction.LDA, AddressingMode.IndirectX, 2, 6),
        //    new (Instruction.LDA, AddressingMode.IndirectY, 2, 5, true),
        //};
    }
}
