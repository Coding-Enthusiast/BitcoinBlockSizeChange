There are many fields in a bitcoin transaction representing a number which is always either 1 or smaller than 253 (which is the biggest amount a 1 byte CompactSize can represent).

If these fields are chaged from UInt to CompactSize it will save up at least 9 bytes per transaction.

The first statistical analysis which I performed using this code shows the ability to free up to 5.5% (or 50 KB) of space in each full block.

Bitcointalk Topic: https://bitcointalk.org/index.php?topic=1700405.0

#Example:
    
    Block Height:   440637
    Tx Count:       1349
    Average TxSize: 555 (bytes)
    Clean Up Size:  29417 (bytes)
    Clean Up %:     3.93%
    Additional Tx:  52
    ====================================
