// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using System.Reflection;

using Microsoft.ServiceFabric.ReliableCollectionBackup.Parser;
using System.Runtime.Remoting.Channels;

namespace Microsoft.ServiceFabric.ReliableCollectionBackup.RestServer
{
    /// <summary>
    /// Wrapper around BackupParser for enumerating over transactions.
    /// </summary>
    public class BackupParserManager : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public BackupParserManager(BackupParser backupParser)
        {
            this.BackupParser = backupParser;

            this.BackupParser.TransactionApplied += (sender, args) =>
            {
                transactionsList = args;
                Console.WriteLine("Transaction Id {0}, CommitSequenceNumber {1}, Changes {2}", args.TransactionId, args.CommitSequenceNumber, args.Changes.Count());
            };
        }

        /// <summary>
        /// Starts parsing the backup without blocking.
        /// </summary>
        /// <returns>Task representing the asynchronous parsing of backup.</returns>
        public Task StartParsing()
        {
            if (this.parsingTask == null)
            {
                // todo : take cancellation timeout from config.
                this.parsingTask = this.BackupParser.ParseAsync(CancellationToken.None);
            }

            return parsingTask;
        }

        /// <summary>
        /// Try to give all transactions.
        /// </summary>
        /// <returns>Returns list of transactions</returns>
        public NotifyTransactionAppliedEventArgs GetTransactions()
        {
            return transactionsList;
        }


        /// <summary>
        /// Checks if parsing operation has finished.
        /// </summary>
        /// <returns>Boolean true/false depending on whether Parsing has completed or not</returns>
        public bool HasParsingFinished()
        {
            return this.parsingTask.IsCompleted;
        }

        /// <summary>
        /// Dispose the Backup manager
        /// </summary>
        public void Dispose()
        {
            this.BackupParser.Dispose();
        }

        internal BackupParser BackupParser { get; private set; }
        
        private Task parsingTask;
        
        private NotifyTransactionAppliedEventArgs transactionsList;
    }
}