function [ sol ] = plotDist(fileName )
%PLOTDIST 
    k = 1;
    % mean true, stand dev true, mean false, stand dev false
    fl = fopen(fileName);
    tline = fgets(fl);
    while ischar(tline)
        splitString = strsplit(tline,',');
        meanTrue(k) = str2double(splitString(1));
        standDevTrue(k) = str2double(splitString(2));
        meanFalse(k) = str2double(splitString(3));
        standDevFalse(k) = str2double(splitString(4));
        meanTrueWz(k) = str2double(splitString(5));
        standDevTrueWz(k) = str2double(splitString(6));
        meanFalseWz(k) = str2double(splitString(7));
        standDevFalseWz(k) = str2double(splitString(8));
        k = k + 1;
        tline = fgets(fl);
    end
    fclose(fl);
    k = k - 1;
    
    clf;
    for j = 1:k
        figure(j);
        subplot(2,1,1);
        if standDevTrue(j) ~= 0 && standDevFalse(j) ~= 0
            hold on;
            title('Statistical Data without Zeros');
            plotGaus(meanTrue(j),standDevTrue(j),1);
            plotGaus(meanFalse(j),standDevFalse(j),2);
            hold off;
        end
        subplot(2,1,2);
        if standDevTrueWz(j) ~= 0 && standDevFalseWz(j) ~= 0
            hold on;
            title('Statistical Data with Zeros');
            plotGaus(meanTrueWz(j),standDevTrueWz(j),1);
            plotGaus(meanFalseWz(j),standDevFalseWz(j),2);
            hold off;
        end
    end
end

function [] = plotGaus(mean,deviation,num)
    X = -2:0.01:2;
    Y = (1/(deviation*sqrt(2*pi)))*exp(-(power(X-mean,2))/(2*power(deviation,2)));
    
    if num == 1
        plot(X,Y,'b-');
    else
       plot(X,Y,'r-'); 
    end
end