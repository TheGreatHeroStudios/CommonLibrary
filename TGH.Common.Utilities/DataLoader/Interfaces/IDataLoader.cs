﻿using System;
using System.Collections.Generic;

namespace TGH.Common.Utilities.DataLoader.Interfaces
{
	public interface IDataLoader<TDataType>
		where TDataType : class
	{
		int ActuaRecordCount { get; }
		int ExpectedRecordCount { get; }

		Func<TDataType, int> KeySelector { get; }

		Func<TDataType, bool> RecordRetrievalPredicate { get; }


		IEnumerable<TDataType> ReadDataIntoMemory();


		void StageDataForInsert(IEnumerable<TDataType> payload, bool deferCommit = false);
	}
}
